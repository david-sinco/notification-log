using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Owners.Enums;
using NotificationLog.RentalService.Domain.Owners.Events;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class OwnerViewProjection : SingleStreamProjection<OwnerView, Guid>
{
    public override OwnerView? Evolve(OwnerView? snapshot, Guid id, IEvent e)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        return e.Data switch
        {
            NaturalOwnerRegistered n => new OwnerView
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
            },
            CompanyOwnerRegistered c => new OwnerView
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
            },
            _ => snapshot
        };
    }
}
