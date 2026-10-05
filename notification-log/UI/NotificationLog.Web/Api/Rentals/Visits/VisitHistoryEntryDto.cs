namespace NotificationLog.Web.Api.Rentals.Visits;

public sealed record VisitHistoryEntryDto(
    DateTimeOffset At,
    string By,
    string Action,
    IReadOnlyList<DateTimeOffset> Slots,
    string? Reason,
    bool IsLate);
