namespace NotificationLog.Web.Api.Rentals.Visits;

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
    string? HostName)
{
    public bool IsNegotiating => Status is nameof(VisitStatus.AwaitingHost) or nameof(VisitStatus.AwaitingVisitor);

    public DateTimeOffset? FirstSlot => ScheduledStartsAt ?? (ProposedSlots.Count > 0 ? ProposedSlots[0] : null);
}

public sealed record RequestVisitRequest(Guid ListingId, IReadOnlyList<DateTimeOffset> Slots);

public sealed record CounterProposeVisitRequest(IReadOnlyList<DateTimeOffset> Slots);

public sealed record ScheduleVisitRequest(DateTimeOffset StartsAt);

public sealed record CancelVisitRequest(string Reason);

public static class VisitLimits
{
    public const int MaxSlots = 3;
    public const int EarliestHour = 7;
    public const int LatestHour = 19;
    public const int MinLeadHours = 24;
    public const int MaxLeadDays = 14;
}
