namespace NotificationLog.RentalService.Infrastructure.Persistence.Reservations;

internal sealed class OwnerPhoneReservation
{
    public required string Id { get; init; }
    public required Guid OwnerId { get; init; }
}
