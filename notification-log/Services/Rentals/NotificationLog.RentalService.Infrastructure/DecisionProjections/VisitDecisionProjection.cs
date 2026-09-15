using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class VisitDecisionProjection : SingleStreamProjection<VisitDecision, Guid>
{
    public override VisitDecision? Evolve(VisitDecision? snapshot, Guid id, IEvent e)
    {
        if (e.Data is VisitRequested requested)
            return new VisitDecision
            {
                Id = id,
                ListingId = requested.ListingId,
                VisitorId = requested.VisitorId,
                HostId = requested.HostId,
                SlotDuration = requested.SlotDuration,
                Status = VisitStatus.Requested
            };

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case VisitConfirmed c:
                snapshot.Status = VisitStatus.Confirmed;
                snapshot.SlotStart = c.SlotStart;
                snapshot.SlotEnd = c.SlotStart + snapshot.SlotDuration;
                break;
            case VisitDeclined: snapshot.Status = VisitStatus.Declined; break;
            case VisitRequestExpired: snapshot.Status = VisitStatus.Expired; break;
            case VisitCompleted: snapshot.Status = VisitStatus.Completed; break;
            case VisitCancelled c:
                snapshot.Status = VisitStatus.Cancelled;
                if (c.By == VisitParty.Visitor && c.IsLate)
                    snapshot.StrikeAt = EventTime.Of(c);
                break;
            case VisitNoShow n:
                snapshot.Status = VisitStatus.NoShow;
                snapshot.StrikeAt = EventTime.Of(n);
                break;
        }

        return snapshot;
    }
}
