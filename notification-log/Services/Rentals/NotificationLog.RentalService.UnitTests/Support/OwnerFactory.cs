using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class OwnerFactory
{
    public static Owner Natural(Guid id)
    {
        var owner = Owner.RegisterNatural(
            id,
            id,
            PersonName.Create("Ana", "Gómez Rincón"),
            IdentityDocument.Create(DocumentType.CitizenshipCard, "52123456"),
            ContactInfo.Create("ana@example.com", "3001234567"));

        owner.ClearDomainEvents();

        return owner;
    }
}
