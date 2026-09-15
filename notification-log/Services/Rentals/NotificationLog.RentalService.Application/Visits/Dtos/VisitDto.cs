namespace NotificationLog.RentalService.Application.Visits.Dtos;

public sealed record VisitDto(
    Guid Id,
    Guid ListingId,
    Guid VisitorId,
    Guid HostId,
    string Status,
    IReadOnlyList<DateTimeOffset> SlotStarts,
    int SlotDurationMinutes,
    DateTimeOffset? ConfirmedSlotStart,
    DateTimeOffset RespondBy,
    string? CancelledBy,
    bool IsLateCancellation,
    string? Reason,
    DateTimeOffset RequestedAt,
    DateTimeOffset UpdatedAt);
