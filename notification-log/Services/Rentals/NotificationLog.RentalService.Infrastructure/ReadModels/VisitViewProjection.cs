using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class VisitViewProjection : SingleStreamProjection<VisitView, Guid>
{
    public override VisitView? Evolve(VisitView? snapshot, Guid id, IEvent e)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        if (e.Data is VisitRequested r)
            return new VisitView
            {
                Id = id,
                ListingId = r.ListingId,
                VisitorId = r.VisitorId,
                HostId = r.HostId,
                Status = VisitStatus.Requested,
                SlotStarts = [.. r.SlotStarts.Order()],
                SlotDurationMinutes = (int)r.SlotDuration.TotalMinutes,
                FirstSlotStart = r.SlotStarts.Min(),
                RespondBy = r.RespondBy,
                RequestedAt = at,
                UpdatedAt = at
            };

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case VisitConfirmed c:
                snapshot.Status = VisitStatus.Confirmed;
                snapshot.ConfirmedSlotStart = c.SlotStart;
                break;
            case VisitDeclined d:
                snapshot.Status = VisitStatus.Declined;
                snapshot.Reason = d.Reason;
                break;
            case VisitRequestExpired: snapshot.Status = VisitStatus.Expired; break;
            case VisitCancelled c:
                snapshot.Status = VisitStatus.Cancelled;
                snapshot.CancelledBy = c.By;
                snapshot.IsLateCancellation = c.IsLate;
                snapshot.Reason = c.Reason;
                break;
            case VisitCompleted: snapshot.Status = VisitStatus.Completed; break;
            case VisitNoShow: snapshot.Status = VisitStatus.NoShow; break;
        }

        snapshot.UpdatedAt = at;

        return snapshot;
    }
}
