using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Listings.Events;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class ListingDecisionProjection : SingleStreamProjection<ListingDecision, Guid>
{
    public override ListingDecision? Evolve(ListingDecision? snapshot, Guid id, IEvent e)
    {
        if (e.Data is ListingDrafted drafted)
            return new ListingDecision
            {
                Id = id,
                PublisherId = drafted.PublisherId,
                AdvisorId = drafted.AdvisorId,
                Operation = drafted.Operation,
                Status = ListingStatus.Draft
            };

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case ListingDetailsUpdated d:
                snapshot.Type = d.Type;
                snapshot.City = d.City;
                snapshot.Neighborhood = d.Neighborhood;
                snapshot.Area = d.Area;
                snapshot.Bedrooms = d.Bedrooms;
                snapshot.Stratum = d.Stratum;
                break;
            case ListingPriceChanged p: snapshot.Price = p.NewPrice; break;
            case ListingSubmittedForReview: snapshot.Status = ListingStatus.InReview; break;
            case ListingApproved a:
                snapshot.Status = ListingStatus.Published;
                snapshot.PublishedAt = EventTime.Of(a);
                break;
            case ListingRejected: snapshot.Status = ListingStatus.Draft; break;
            case ListingPaused: snapshot.Status = ListingStatus.Paused; break;
            case ListingResumed: snapshot.Status = ListingStatus.Published; break;
            case ListingRenewed: snapshot.Status = ListingStatus.Published; break;
            case ListingExpired: snapshot.Status = ListingStatus.Expired; break;
            case AdvisorAssigned assigned: snapshot.AdvisorId = assigned.AdvisorId; break;
            case AdvisorUnassigned: snapshot.AdvisorId = null; break;
            case ListingReserved: snapshot.Status = ListingStatus.Reserved; break;
            case ReservationReleased: snapshot.Status = ListingStatus.Published; break;
            case ListingClosed: snapshot.Status = ListingStatus.Closed; break;
            case ListingWithdrawn: snapshot.Status = ListingStatus.Withdrawn; break;
            case ListingSuspended: snapshot.Status = ListingStatus.Suspended; break;
            case ListingReinstated: snapshot.Status = ListingStatus.Published; break;
        }

        return snapshot;
    }
}
