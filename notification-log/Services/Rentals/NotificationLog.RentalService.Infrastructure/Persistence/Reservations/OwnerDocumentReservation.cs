namespace NotificationLog.RentalService.Infrastructure.Persistence.Reservations;

internal sealed class OwnerDocumentReservation
{
    public required string Id { get; init; }
    public required Guid OwnerId { get; init; }
}
