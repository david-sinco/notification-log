using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Common.ValueObjects;

public sealed record TimeSlot
{
    public const int FirstHour = 7;
    public const int LastHour = 19;

    public static readonly TimeSpan Duration = TimeSpan.FromHours(1);

    private static readonly TimeZoneInfo Colombia = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

    public DateTimeOffset StartsAt { get; }
    public DateTimeOffset EndsAt => StartsAt + Duration;

    private TimeSlot(DateTimeOffset startsAt) => StartsAt = startsAt;

    public static TimeSlot Create(DateTimeOffset startsAt)
    {
        var start = TimeZoneInfo.ConvertTime(startsAt, Colombia).TimeOfDay;
        var end = start + Duration;

        if (start < TimeSpan.FromHours(FirstHour) || end > TimeSpan.FromHours(LastHour))
            throw new DomainException(
                $"La franja debe estar entre las {FirstHour}:00 y las {LastHour}:00 hora de Colombia.");

        return new TimeSlot(startsAt.ToUniversalTime());
    }

    internal static TimeSlot FromStorage(DateTimeOffset startsAt) => new(startsAt);

    public bool Overlaps(TimeSlot other) => StartsAt < other.EndsAt && other.StartsAt < EndsAt;

    public override string ToString() => $"{TimeZoneInfo.ConvertTime(StartsAt, Colombia):yyyy-MM-dd HH:mm}";
}
