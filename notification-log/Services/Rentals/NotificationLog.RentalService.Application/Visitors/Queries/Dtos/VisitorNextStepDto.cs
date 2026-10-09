namespace NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

public sealed record VisitorNextStepDto(
    Guid VisitId,
    string Status,
    DateTimeOffset At,
    string? ListingNeighborhood);
