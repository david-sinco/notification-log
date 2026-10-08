using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Projections;

public sealed class VisitViewProjection : SingleStreamProjection<VisitView, Guid>
{
    public override async ValueTask<VisitView?> EvolveAsync(
        VisitView? snapshot, Guid id, IQuerySession session, IEvent e, CancellationToken cancellation)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        if (e.Data is VisitRequested requested)
        {
            var listing = await session.LoadAsync<ListingView>(requested.ListingId, cancellation);
            var visitor = await session.LoadAsync<VisitorView>(requested.VisitorId, cancellation);

            return new VisitView
            {
                Id = id,
                ListingId = requested.ListingId,
                HostId = requested.HostId,
                VisitorId = requested.VisitorId,
                ListingType = listing?.Type,
                ListingNeighborhood = listing?.Neighborhood,
                ListingCity = listing?.City,
                VisitorName = visitor?.Name ?? string.Empty,
                HostName = listing?.OwnerName ?? string.Empty,
                Status = VisitStatus.AwaitingHost,
                ProposedSlots = [.. requested.Slots],
                RespondBy = requested.RespondBy,
                RequestedAt = at,
                UpdatedAt = at,
                History = [new VisitHistoryEntry { At = at, By = VisitParty.Visitor, Action = "Requested", Slots = [.. requested.Slots] }]
            };
        }

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case VisitCounterProposed p:
                snapshot.Status = p.By == VisitParty.Host ? VisitStatus.AwaitingVisitor : VisitStatus.AwaitingHost;
                snapshot.ProposedSlots = [.. p.Slots];
                snapshot.RespondBy = p.RespondBy;
                snapshot.History.Add(new VisitHistoryEntry { At = at, By = p.By, Action = "CounterProposed", Slots = [.. p.Slots] });
                break;
            case VisitScheduled s:
                EndNegotiation(snapshot, VisitStatus.Scheduled);
                snapshot.ScheduledStartsAt = s.StartsAt;
                snapshot.ScheduledEndsAt = s.StartsAt + TimeSlot.Duration;
                snapshot.History.Add(new VisitHistoryEntry { At = at, By = s.By, Action = "Scheduled", Slots = [s.StartsAt] });
                break;
            case VisitCancelled c:
                EndNegotiation(snapshot, VisitStatus.Cancelled);
                snapshot.CancelledBy = c.By;
                snapshot.CancellationReason = c.Reason;
                snapshot.IsLateCancellation = c.IsLate;
                snapshot.History.Add(new VisitHistoryEntry { At = at, By = c.By, Action = "Cancelled", Reason = c.Reason, IsLate = c.IsLate });
                break;
            case VisitExpired:
                EndNegotiation(snapshot, VisitStatus.Expired);
                snapshot.History.Add(new VisitHistoryEntry { At = at, By = VisitParty.System, Action = "Expired" });
                break;
            case VisitCompleted c:
                snapshot.Status = VisitStatus.Completed;
                snapshot.ClosedBy = c.By;
                snapshot.History.Add(new VisitHistoryEntry { At = at, By = c.By, Action = "Completed" });
                break;
            case VisitNoShow:
                snapshot.Status = VisitStatus.NoShow;
                snapshot.ClosedBy = VisitParty.Host;
                snapshot.History.Add(new VisitHistoryEntry { At = at, By = VisitParty.Host, Action = "NoShow" });
                break;
        }

        snapshot.UpdatedAt = at;

        return snapshot;
    }

    private static void EndNegotiation(VisitView snapshot, VisitStatus status)
    {
        snapshot.Status = status;
        snapshot.ProposedSlots.Clear();
        snapshot.RespondBy = null;
    }
}
