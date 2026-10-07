namespace NotificationLog.RentalService.Infrastructure.Persistence.Reservations;

internal sealed class OwnerEmailReservation
{
    public required string Id { get; init; }
    public required Guid OwnerId { get; init; }
}
