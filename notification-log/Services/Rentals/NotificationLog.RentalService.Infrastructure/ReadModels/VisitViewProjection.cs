using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class VisitViewProjection : SingleStreamProjection<VisitView, Guid>
{
    public override VisitView? Evolve(VisitView? snapshot, Guid id, IEvent e)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        if (e.Data is VisitRequested requested)
            return new VisitView
            {
                Id = id,
                ListingId = requested.ListingId,
                HostId = requested.HostId,
                VisitorId = requested.VisitorId,
                Status = VisitStatus.AwaitingHost,
                ProposedSlots = [.. requested.Slots],
                RespondBy = requested.RespondBy,
                RequestedAt = at,
                UpdatedAt = at
            };

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case VisitCounterProposed p:
                snapshot.Status = p.By == VisitParty.Host ? VisitStatus.AwaitingVisitor : VisitStatus.AwaitingHost;
                snapshot.ProposedSlots = [.. p.Slots];
                snapshot.RespondBy = p.RespondBy;
                break;
            case VisitScheduled s:
                EndNegotiation(snapshot, VisitStatus.Scheduled);
                snapshot.ScheduledStartsAt = s.StartsAt;
                snapshot.ScheduledEndsAt = s.StartsAt + TimeSlot.Duration;
                break;
            case VisitCancelled c:
                EndNegotiation(snapshot, VisitStatus.Cancelled);
                snapshot.CancelledBy = c.By;
                snapshot.CancellationReason = c.Reason;
                snapshot.IsLateCancellation = c.IsLate;
                break;
            case VisitExpired:
                EndNegotiation(snapshot, VisitStatus.Expired);
                break;
            case VisitCompleted c:
                snapshot.Status = VisitStatus.Completed;
                snapshot.ClosedBy = c.By;
                break;
            case VisitNoShow:
                snapshot.Status = VisitStatus.NoShow;
                snapshot.ClosedBy = VisitParty.Host;
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
