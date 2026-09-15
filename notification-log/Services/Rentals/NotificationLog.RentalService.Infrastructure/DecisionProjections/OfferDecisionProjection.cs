using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Offers.Enums;
using NotificationLog.RentalService.Domain.Offers.Events;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class OfferDecisionProjection : SingleStreamProjection<OfferDecision, Guid>
{
    public override OfferDecision? Evolve(OfferDecision? snapshot, Guid id, IEvent e)
    {
        if (e.Data is OfferSubmitted submitted)
            return new OfferDecision
            {
                Id = id,
                ListingId = submitted.ListingId,
                OffererId = submitted.OffererId,
                Status = OfferStatus.AwaitingPublisher,
                IsOpen = true
            };

        if (snapshot is null)
            return null;

        snapshot.Status = e.Data switch
        {
            OfferCountered c => c.By == OfferParty.Publisher ? OfferStatus.AwaitingOfferer : OfferStatus.AwaitingPublisher,
            OfferAccepted => OfferStatus.Accepted,
            OfferRejected => OfferStatus.Rejected,
            OfferWithdrawn => OfferStatus.Withdrawn,
            OfferExpired => OfferStatus.Expired,
            OfferFellThrough => OfferStatus.FellThrough,
            _ => snapshot.Status
        };

        snapshot.IsOpen = snapshot.Status is OfferStatus.AwaitingPublisher or OfferStatus.AwaitingOfferer;

        return snapshot;
    }
}
