namespace NotificationLog.RentalService.Application.Visits.Queries.Dtos;

public sealed record VisitDto(
    Guid Id,
    Guid ListingId,
    Guid HostId,
    Guid VisitorId,
    string Status,
    IReadOnlyList<DateTimeOffset> ProposedSlots,
    DateTimeOffset? RespondBy,
    DateTimeOffset? ScheduledStartsAt,
    DateTimeOffset? ScheduledEndsAt,
    string? CancelledBy,
    string? CancellationReason,
    bool IsLateCancellation,
    string? ClosedBy,
    DateTimeOffset RequestedAt,
    DateTimeOffset UpdatedAt,
    string? ListingType,
    string? ListingNeighborhood,
    string? ListingCity,
    string? VisitorName,
    string? HostName);
