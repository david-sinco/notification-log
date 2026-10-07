using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Listings.Events;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Projections;

public sealed class ListingViewProjection : SingleStreamProjection<ListingView, Guid>
{
    public override async ValueTask<ListingView?> EvolveAsync(
        ListingView? snapshot, Guid id, IQuerySession session, IEvent e, CancellationToken cancellation)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        if (e.Data is ListingDrafted drafted)
        {
            var owner = await session.LoadAsync<OwnerView>(drafted.OwnerId, cancellation);

            return new ListingView
            {
                Id = id,
                OwnerId = drafted.OwnerId,
                OwnerName = owner?.Name ?? string.Empty,
                CreatedBy = drafted.CreatedBy,
                Operation = drafted.Operation,
                Status = ListingStatus.Draft,
                CreatedAt = at,
                UpdatedAt = at
            };
        }

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case ListingDetailsUpdated d:
                snapshot.Type = d.Type;
                snapshot.Area = d.Area;
                snapshot.Bedrooms = d.Bedrooms;
                snapshot.Bathrooms = d.Bathrooms;
                snapshot.ParkingSpots = d.ParkingSpots;
                snapshot.Stratum = d.Stratum;
                snapshot.Floor = d.Floor;
                snapshot.HasElevator = d.HasElevator;
                snapshot.AdministrationFee = d.AdministrationFee;
                snapshot.City = d.City;
                snapshot.Neighborhood = d.Neighborhood;
                snapshot.Address = d.Address;
                snapshot.Description = d.Description;
                break;
            case ListingPhotosUpdated p: snapshot.Photos = [.. p.Photos]; break;
            case ListingPriceChanged p: snapshot.Price = p.NewPrice; break;
            case ListingSubmittedForReview: snapshot.Status = ListingStatus.InReview; break;
            case ListingApproved a: Publish(snapshot, a.ExpiresAt); break;
            case ListingRejected r:
                snapshot.Status = ListingStatus.Draft;
                snapshot.RejectionReasons = [.. r.Reasons];
                break;
            case ListingPaused: snapshot.Status = ListingStatus.Paused; break;
            case ListingResumed: snapshot.Status = ListingStatus.Published; break;
            case ListingRenewed r: Publish(snapshot, r.ExpiresAt); break;
            case ListingExpired: snapshot.Status = ListingStatus.Expired; break;
            case ListingClosed c:
                snapshot.Status = ListingStatus.Closed;
                snapshot.FinalPrice = c.FinalPrice;
                snapshot.SignedOn = c.SignedOn;
                break;
            case ListingWithdrawn w:
                snapshot.Status = ListingStatus.Withdrawn;
                snapshot.StatusReason = w.Reason;
                break;
            case ListingSuspended s:
                snapshot.Status = ListingStatus.Suspended;
                snapshot.StatusReason = s.Reason;
                break;
            case ListingReinstated:
                snapshot.Status = ListingStatus.Published;
                snapshot.StatusReason = null;
                break;
        }

        snapshot.UpdatedAt = at;

        return snapshot;
    }

    private static void Publish(ListingView snapshot, DateTimeOffset expiresAt)
    {
        snapshot.Status = ListingStatus.Published;
        snapshot.ExpiresAt = expiresAt;
        snapshot.RejectionReasons.Clear();
        snapshot.StatusReason = null;
    }
}
