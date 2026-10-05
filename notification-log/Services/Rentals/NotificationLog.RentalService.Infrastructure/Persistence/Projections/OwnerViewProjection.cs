using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Projections;
using NotificationLog.RentalService.Domain.Listings.Events;
using NotificationLog.RentalService.Domain.Owners.Enums;
using NotificationLog.RentalService.Domain.Owners.Events;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Projections;

public sealed class OwnerViewProjection : MultiStreamProjection<OwnerView, Guid>
{
    public OwnerViewProjection()
    {
        Identity<NaturalOwnerRegistered>(e => e.OwnerId);
        Identity<CompanyOwnerRegistered>(e => e.OwnerId);
        Identity<ListingDrafted>(e => e.OwnerId);
    }

    public override OwnerView? Evolve(OwnerView? snapshot, Guid id, IEvent e)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        switch (e.Data)
        {
            case NaturalOwnerRegistered n:
                return new OwnerView
                {
                    Id = id,
                    CreatedBy = n.CreatedBy,
                    Type = OwnerType.Natural,
                    FirstNames = n.FirstNames,
                    LastNames = n.LastNames,
                    DocumentType = n.DocumentType,
                    DocumentNumber = n.DocumentNumber,
                    Email = n.Email,
                    Phone = n.Phone,
                    RegisteredAt = at
                };
            case CompanyOwnerRegistered c:
                return new OwnerView
                {
                    Id = id,
                    CreatedBy = c.CreatedBy,
                    Type = OwnerType.Company,
                    LegalName = c.LegalName,
                    Nit = c.Nit,
                    NitCheckDigit = c.NitCheckDigit,
                    Email = c.Email,
                    Phone = c.Phone,
                    RegisteredAt = at
                };
            case ListingDrafted when snapshot is not null:
                snapshot.ListingCount++;
                return snapshot;
            default:
                return snapshot;
        }
    }
}
