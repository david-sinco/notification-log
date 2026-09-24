namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class VisitorDocumentReservation
{
    public required string Id { get; init; }
    public required Guid VisitorId { get; init; }
}
