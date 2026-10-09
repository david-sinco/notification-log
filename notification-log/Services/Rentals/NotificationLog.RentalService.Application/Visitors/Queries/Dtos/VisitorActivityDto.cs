namespace NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

public sealed record VisitorActivityDto(
    DateTimeOffset At,
    string Action,
    string? By,
    Guid? VisitId,
    string? ListingNeighborhood,
    string? HostName,
    DateTimeOffset? Slot,
    bool IsLate);
