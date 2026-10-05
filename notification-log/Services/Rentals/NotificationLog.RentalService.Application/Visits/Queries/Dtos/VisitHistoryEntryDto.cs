namespace NotificationLog.RentalService.Application.Visits.Queries.Dtos;

public sealed record VisitHistoryEntryDto(
    DateTimeOffset At,
    string By,
    string Action,
    IReadOnlyList<DateTimeOffset> Slots,
    string? Reason,
    bool IsLate);
