namespace NotificationLog.Web.Api.Rentals.Visits;

public sealed record VisitSummaryDto(
    Guid Id,
    Guid ListingId,
    Guid VisitorId,
    Guid HostId,
    string Status,
    DateTimeOffset FirstSlotStart,
    DateTimeOffset? ConfirmedSlotStart,
    DateTimeOffset RespondBy,
    DateTimeOffset UpdatedAt);

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

public sealed record RequestVisitRequest(Guid VisitorId, Guid ListingId, IReadOnlyList<DateTimeOffset> SlotStarts);

public sealed record ConfirmVisitRequest(Guid ActorId, DateTimeOffset SlotStart);

public sealed record VisitReasonRequest(Guid ActorId, string Reason);

public sealed record ReportVisitOutcomeRequest(Guid ActorId, bool Attended);

public static class VisitLimits
{
    public const int MaxSlots = 3;
    public const int EarliestHour = 7;
    public const int LatestHour = 19;
}
