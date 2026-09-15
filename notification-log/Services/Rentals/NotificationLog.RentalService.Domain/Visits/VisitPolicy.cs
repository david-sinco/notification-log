namespace NotificationLog.RentalService.Domain.Visits;

public static class VisitPolicy
{
    public const int MaxSlots = 3;
    public const int MaxReasonLength = 500;
    public const int MaxPendingVisits = 3;
    public const int MaxStrikes = 2;

    public static readonly TimeSpan SlotDuration = TimeSpan.FromHours(1);
    public static readonly TimeSpan MinLeadTime = TimeSpan.FromHours(2);
    public static readonly TimeSpan MaxLeadTime = TimeSpan.FromDays(14);
    public static readonly TimeSpan EarliestStart = TimeSpan.FromHours(7);
    public static readonly TimeSpan LatestEnd = TimeSpan.FromHours(19);
    public static readonly TimeSpan ColombiaOffset = TimeSpan.FromHours(-5);
    public static readonly TimeSpan ResponseWindow = TimeSpan.FromHours(48);
    public static readonly TimeSpan LateCancellationWindow = TimeSpan.FromHours(12);
    public static readonly TimeSpan AutoCompleteAfter = TimeSpan.FromHours(72);
    public static readonly TimeSpan StrikeWindow = TimeSpan.FromDays(30);
    public static readonly TimeSpan VisitBan = TimeSpan.FromDays(15);
}
