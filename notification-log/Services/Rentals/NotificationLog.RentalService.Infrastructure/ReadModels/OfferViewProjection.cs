using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Offers.Enums;
using NotificationLog.RentalService.Domain.Offers.Events;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class OfferViewProjection : SingleStreamProjection<OfferView, Guid>
{
    public override OfferView? Evolve(OfferView? snapshot, Guid id, IEvent e)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        if (e.Data is OfferSubmitted s)
            return new OfferView
            {
                Id = id,
                ListingId = s.ListingId,
                OffererId = s.OffererId,
                Operation = s.Operation,
                Status = OfferStatus.AwaitingPublisher,
                ListedPrice = s.ListedPrice,
                Amount = s.Amount,
                RentStartDate = s.RentStartDate,
                RentTermMonths = s.RentTermMonths,
                LastMoveBy = OfferParty.Offerer,
                RespondBy = s.RespondBy,
                SubmittedAt = at,
                UpdatedAt = at,
                History = [new OfferMoveView { Kind = "Submitted", By = OfferParty.Offerer, Amount = s.Amount, At = at }]
            };

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case OfferCountered c:
                snapshot.Amount = c.Amount;
                snapshot.LastMoveBy = c.By;
                snapshot.CounterOfferCount++;
                snapshot.RespondBy = c.RespondBy;
                snapshot.Status = c.By == OfferParty.Publisher ? OfferStatus.AwaitingOfferer : OfferStatus.AwaitingPublisher;
                snapshot.History.Add(new OfferMoveView { Kind = "Countered", By = c.By, Amount = c.Amount, At = at });
                break;
            case OfferAccepted a:
                snapshot.LastMoveBy = a.By;
                snapshot.Status = OfferStatus.Accepted;
                snapshot.History.Add(new OfferMoveView { Kind = "Accepted", By = a.By, At = at });
                break;
            case OfferRejected r:
                snapshot.Status = OfferStatus.Rejected;
                snapshot.History.Add(new OfferMoveView { Kind = "Rejected", Reason = r.Reason, At = at });
                break;
            case OfferWithdrawn:
                snapshot.Status = OfferStatus.Withdrawn;
                snapshot.History.Add(new OfferMoveView { Kind = "Withdrawn", By = OfferParty.Offerer, At = at });
                break;
            case OfferExpired:
                snapshot.Status = OfferStatus.Expired;
                snapshot.History.Add(new OfferMoveView { Kind = "Expired", At = at });
                break;
            case OfferFellThrough f:
                snapshot.Status = OfferStatus.FellThrough;
                snapshot.History.Add(new OfferMoveView { Kind = "FellThrough", Reason = f.Reason, At = at });
                break;
        }

        snapshot.UpdatedAt = at;

        return snapshot;
    }
}
