using System.Globalization;
using NotificationLog.RentalService.Domain.Common.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class Clock
{
    private static readonly TimeSpan Colombia = TimeSpan.FromHours(-5);

    public static readonly DateTimeOffset Now = At("2026-10-05 09:00");

    public static DateTimeOffset At(string moment)
        => new(DateTime.ParseExact(moment, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), Colombia);

    public static TimeSlot Slot(string moment) => TimeSlot.Create(At(moment));

    public static List<TimeSlot> Slots(string moments)
        => moments.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(Slot).ToList();
}
