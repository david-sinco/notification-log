namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed record VisitorNextStepDto(
    Guid VisitId,
    string Status,
    DateTimeOffset At,
    string? ListingNeighborhood);
