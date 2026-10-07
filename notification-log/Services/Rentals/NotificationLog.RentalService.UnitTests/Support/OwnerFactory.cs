using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class OwnerFactory
{
    public const string Email = "ana@example.com";

    public static Owner Registered(Guid id) => Registered(id, id);

    public static Owner Registered(Guid id, Guid? relatedUserId)
    {
        var owner = Owner.Register(
            id,
            Guid.NewGuid(),
            relatedUserId,
            OwnerName.Create("Ana Gómez Rincón"),
            ContactInfo.Create(Email, "3001234567"));

        owner.ClearDomainEvents();

        return owner;
    }
}
