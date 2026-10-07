using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Projections;
using NotificationLog.RentalService.Domain.Listings.Events;
using NotificationLog.RentalService.Domain.Owners.Events;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Projections;

public sealed class OwnerViewProjection : MultiStreamProjection<OwnerView, Guid>
{
    public OwnerViewProjection()
    {
        Identity<OwnerRegistered>(e => e.OwnerId);
        Identity<OwnerClaimed>(e => e.OwnerId);
        Identity<ListingDrafted>(e => e.OwnerId);
    }

    public override OwnerView? Evolve(OwnerView? snapshot, Guid id, IEvent e)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        switch (e.Data)
        {
            case OwnerRegistered r:
                return new OwnerView
                {
                    Id = id,
                    CreatedBy = r.CreatedBy,
                    RelatedUserId = r.RelatedUserId,
                    Name = r.Name,
                    Email = r.Email,
                    Phone = r.Phone,
                    RegisteredAt = at
                };
            case OwnerClaimed claimed when snapshot is not null:
                snapshot.RelatedUserId = claimed.UserId;
                return snapshot;
            case ListingDrafted when snapshot is not null:
                snapshot.ListingCount++;
                return snapshot;
            default:
                return snapshot;
        }
    }
}
