using Domain.Shared.EventSourcing;
using JasperFx.Events;
using JasperFx.Events.Grouping;
using Marten;
using Marten.Events.Projections;
using NotificationLog.RentalService.Domain.Visitors.Events;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Projections;

public sealed class VisitorViewProjection : MultiStreamProjection<VisitorView, Guid>
{
    public VisitorViewProjection()
    {
        Identity<VisitorRegistered>(e => e.VisitorId);
        Identity<VisitorUpdated>(e => e.VisitorId);
        Identity<VisitRequested>(e => e.VisitorId);

        CustomGrouping(GroupVisitEventsByVisitorAsync);
    }

    public override async ValueTask<VisitorView?> EvolveAsync(
        VisitorView? snapshot, Guid id, IQuerySession session, IEvent e, CancellationToken cancellation)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        if (e.Data is VisitorRegistered registered)
        {
            return new VisitorView
            {
                Id = id,
                Name = registered.Name,
                Email = registered.Email,
                Phone = registered.Phone,
                RegisteredAt = at,
                Activity = [new VisitorActivityEntry { At = at, Action = "Registered" }]
            };
        }

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case VisitorUpdated updated:
                ApplyUpdate(snapshot, updated, at);
                return snapshot;
            case VisitRequested requested:
                await ApplyRequestAsync(snapshot, requested, session, at, cancellation);
                return snapshot;
        }

        if (snapshot.Visits.Find(visit => visit.VisitId == e.StreamId) is not { } visit)
            return snapshot;

        switch (e.Data)
        {
            case VisitCounterProposed p:
                visit.Status = p.By == VisitParty.Host ? VisitStatus.AwaitingVisitor : VisitStatus.AwaitingHost;
                visit.ProposedSlots = [.. p.Slots];
                visit.RespondBy = p.RespondBy;
                snapshot.Activity.Add(Entry(at, "CounterProposed", p.By, visit, p.Slots.FirstOrDefault()));
                break;
            case VisitScheduled s:
                EndNegotiation(visit, VisitStatus.Scheduled);
                visit.ScheduledStartsAt = s.StartsAt;
                snapshot.Activity.Add(Entry(at, "Scheduled", s.By, visit, s.StartsAt));
                break;
            case VisitCancelled c:
                EndNegotiation(visit, VisitStatus.Cancelled);
                snapshot.Activity.Add(Entry(at, "Cancelled", c.By, visit, isLate: c.IsLate));
                break;
            case VisitExpired:
                EndNegotiation(visit, VisitStatus.Expired);
                snapshot.Activity.Add(Entry(at, "Expired", VisitParty.System, visit));
                break;
            case VisitCompleted c:
                visit.Status = VisitStatus.Completed;
                snapshot.Activity.Add(Entry(at, "Completed", c.By, visit));
                break;
            case VisitNoShow:
                visit.Status = VisitStatus.NoShow;
                snapshot.NoShowCount++;
                snapshot.Activity.Add(Entry(at, "NoShow", VisitParty.Host, visit));
                break;
        }

        visit.UpdatedAt = at;

        return snapshot;
    }

    private static async Task GroupVisitEventsByVisitorAsync(
        IQuerySession session, IReadOnlyList<IEvent> events, IEventGrouping<Guid> grouping)
    {
        var followUps = events
            .Where(e => e.Data is VisitCounterProposed or VisitScheduled or VisitCancelled or VisitExpired or VisitCompleted or VisitNoShow)
            .ToList();

        if (followUps.Count == 0)
            return;

        var visitorByVisit = new Dictionary<Guid, Guid>();

        foreach (var requested in events.Select(e => e.Data).OfType<VisitRequested>())
            visitorByVisit[requested.VisitId] = requested.VisitorId;

        var unknownVisits = followUps.Select(e => e.StreamId).Where(id => !visitorByVisit.ContainsKey(id)).Distinct().ToArray();

        if (unknownVisits.Length > 0)
        {
            var requests = await session.Events.QueryRawEventDataOnly<VisitRequested>()
                .Where(requested => requested.VisitId.IsOneOf(unknownVisits))
                .ToListAsync();

            foreach (var requested in requests)
                visitorByVisit[requested.VisitId] = requested.VisitorId;
        }

        foreach (var e in followUps)
        {
            if (visitorByVisit.TryGetValue(e.StreamId, out var visitorId))
                grouping.AddEvent(visitorId, e);
        }
    }

    private static void ApplyUpdate(VisitorView snapshot, VisitorUpdated updated, DateTimeOffset at)
    {
        if (updated.Name != snapshot.Name)
            snapshot.Activity.Add(new VisitorActivityEntry { At = at, Action = "NameChanged" });

        if (updated.Email != snapshot.Email)
            snapshot.Activity.Add(new VisitorActivityEntry { At = at, Action = "EmailChanged" });

        if (updated.Phone != snapshot.Phone)
            snapshot.Activity.Add(new VisitorActivityEntry { At = at, Action = "PhoneChanged" });

        snapshot.Name = updated.Name;
        snapshot.Email = updated.Email;
        snapshot.Phone = updated.Phone;
    }

    private static async Task ApplyRequestAsync(
        VisitorView snapshot, VisitRequested requested, IQuerySession session, DateTimeOffset at, CancellationToken cancellation)
    {
        var listing = await session.LoadAsync<ListingView>(requested.ListingId, cancellation);

        var visit = new VisitorVisitEntry
        {
            VisitId = requested.VisitId,
            ListingId = requested.ListingId,
            ListingType = listing?.Type,
            ListingNeighborhood = listing?.Neighborhood,
            ListingCity = listing?.City,
            HostName = listing?.OwnerName ?? string.Empty,
            Status = VisitStatus.AwaitingHost,
            ProposedSlots = [.. requested.Slots],
            RespondBy = requested.RespondBy,
            RequestedAt = at,
            UpdatedAt = at
        };

        snapshot.VisitCount++;
        snapshot.Visits.Add(visit);
        snapshot.Activity.Add(Entry(at, "Requested", VisitParty.Visitor, visit, requested.Slots.FirstOrDefault()));
    }

    private static void EndNegotiation(VisitorVisitEntry visit, VisitStatus status)
    {
        visit.Status = status;
        visit.ProposedSlots.Clear();
        visit.RespondBy = null;
    }

    private static VisitorActivityEntry Entry(
        DateTimeOffset at, string action, VisitParty by, VisitorVisitEntry visit, DateTimeOffset? slot = null, bool isLate = false) => new()
    {
        At = at,
        Action = action,
        By = by,
        VisitId = visit.VisitId,
        ListingNeighborhood = visit.ListingNeighborhood,
        HostName = visit.HostName,
        Slot = slot,
        IsLate = isLate
    };
}
