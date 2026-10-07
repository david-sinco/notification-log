using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class OwnerFactory
{
    public const string Email = "ana@example.com";

    public static Owner Natural(Guid id) => Natural(id, id);

    public static Owner Natural(Guid id, Guid? relatedUserId)
    {
        var owner = Owner.RegisterNatural(
            id,
            Guid.NewGuid(),
            relatedUserId,
            PersonName.Create("Ana", "Gómez Rincón"),
            IdentityDocument.Create(DocumentType.CitizenshipCard, "52123456"),
            ContactInfo.Create(Email, "3001234567"));

        owner.ClearDomainEvents();

        return owner;
    }
}
