namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class OwnerDocumentReservation
{
    public required string Id { get; init; }
    public required Guid OwnerId { get; init; }
}
