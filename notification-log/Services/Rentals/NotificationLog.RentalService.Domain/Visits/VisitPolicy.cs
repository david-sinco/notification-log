namespace NotificationLog.RentalService.Domain.Visits;

public static class VisitPolicy
{
    public const int MaxProposedSlots = 3;
    public const int MaxNoteLength = 500;
    public const int MaxReasonLength = 500;

    public static readonly TimeSpan ResponseTime = TimeSpan.FromHours(48);
    public static readonly TimeSpan MinLeadTime = TimeSpan.FromHours(24);
    public static readonly TimeSpan MaxLeadTime = TimeSpan.FromDays(14);
    public static readonly TimeSpan LateCancellationWindow = TimeSpan.FromHours(12);
    public static readonly TimeSpan AutoCompleteAfter = TimeSpan.FromHours(72);

    public static DateTimeOffset RespondByFrom(DateTimeOffset now, DateTimeOffset earliestSlot)
    {
        var byResponseTime = now + ResponseTime;
        var beforeSlot = earliestSlot - MinLeadTime;

        return byResponseTime < beforeSlot ? byResponseTime : beforeSlot;
    }
}
