using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Common;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners.Events;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Domain.Owners;

public sealed class Owner : AggregateRoot
{
    private Owner() { }

    public Guid CreatedBy { get; private set; }
    public Guid? RelatedUserId { get; private set; }
    public OwnerName? Name { get; private set; }
    public ContactInfo? Contact { get; private set; }

    public Guid HostUserId => RelatedUserId ?? CreatedBy;

    public static Owner Register(Guid id, Guid createdBy, Guid? relatedUserId, OwnerName name, ContactInfo contact)
    {
        Guard.RequireId(id, "El identificador del propietario es obligatorio.");
        Guard.RequireId(createdBy, "El usuario que registra al propietario es obligatorio.");
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(contact);

        var owner = new Owner();

        owner.Raise(new OwnerRegistered(id, createdBy, name.Value, contact.Email, contact.Phone, relatedUserId));

        return owner;
    }

    public void Claim(Guid userId, string? verifiedEmail, string? verifiedPhone)
    {
        Guard.RequireId(userId, "El usuario que reclama al propietario es obligatorio.");

        if (RelatedUserId is not null)
            throw new DomainException("El propietario ya fue reclamado.");

        var emailMatches = !string.IsNullOrEmpty(verifiedEmail) && verifiedEmail == Contact?.Email;
        var phoneMatches = !string.IsNullOrEmpty(verifiedPhone) && verifiedPhone == Contact?.Phone;

        if (!emailMatches && !phoneMatches)
            throw new DomainException("Tu correo o teléfono confirmado no coincide con el del propietario.");

        Raise(new OwnerClaimed(Id, userId));
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case OwnerRegistered e: When(e); break;
            case OwnerClaimed e: When(e); break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Owner.");
        }
    }

    private void When(OwnerRegistered e)
    {
        Id = e.OwnerId;
        CreatedBy = e.CreatedBy;
        RelatedUserId = e.RelatedUserId;
        Name = OwnerName.FromStorage(e.Name);
        Contact = ContactInfo.FromStorage(e.Email, e.Phone);
    }

    private void When(OwnerClaimed e) => RelatedUserId = e.UserId;
}
