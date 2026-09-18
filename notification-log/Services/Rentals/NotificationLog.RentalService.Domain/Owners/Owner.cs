using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Owners.Enums;
using NotificationLog.RentalService.Domain.Owners.Events;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Owners;

public sealed class Owner : AggregateRoot
{
    private Owner() { }

    public Guid CreatedBy { get; private set; }
    public OwnerType Type { get; private set; }
    public PersonName? Name { get; private set; }
    public IdentityDocument? Document { get; private set; }
    public LegalName? LegalName { get; private set; }
    public Nit? Nit { get; private set; }

    public static Owner RegisterNatural(Guid id, Guid createdBy, PersonName name, IdentityDocument document)
    {
        Guard.RequireId(id, "El identificador del propietario es obligatorio.");
        Guard.RequireId(createdBy, "El usuario que registra al propietario es obligatorio.");
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(document);

        var owner = new Owner();

        owner.Raise(new NaturalOwnerRegistered(
            id, createdBy, name.FirstNames, name.LastNames, document.Type, document.Number));

        return owner;
    }

    public static Owner RegisterCompany(Guid id, Guid createdBy, LegalName legalName, Nit nit)
    {
        Guard.RequireId(id, "El identificador del propietario es obligatorio.");
        Guard.RequireId(createdBy, "El usuario que registra al propietario es obligatorio.");
        ArgumentNullException.ThrowIfNull(legalName);
        ArgumentNullException.ThrowIfNull(nit);

        var owner = new Owner();

        owner.Raise(new CompanyOwnerRegistered(id, createdBy, legalName.Value, nit.Number, nit.CheckDigit));

        return owner;
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case NaturalOwnerRegistered e: When(e); break;
            case CompanyOwnerRegistered e: When(e); break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Owner.");
        }
    }

    private void When(NaturalOwnerRegistered e)
    {
        Id = e.OwnerId;
        CreatedBy = e.CreatedBy;
        Type = OwnerType.Natural;
        Name = PersonName.FromStorage(e.FirstNames, e.LastNames);
        Document = IdentityDocument.FromStorage(e.DocumentType, e.DocumentNumber);
    }

    private void When(CompanyOwnerRegistered e)
    {
        Id = e.OwnerId;
        CreatedBy = e.CreatedBy;
        Type = OwnerType.Company;
        LegalName = LegalName.FromStorage(e.LegalName);
        Nit = Nit.FromStorage(e.Nit, e.NitCheckDigit);
    }
}
