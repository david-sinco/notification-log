using System.Globalization;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public static class ColombiaTime
{
    private static readonly TimeSpan Offset = TimeSpan.FromHours(-5);

    public static DateTimeOffset Parse(string moment)
        => new(DateTime.ParseExact(moment, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), Offset);

    public static DateOnly DateOf(DateTimeOffset moment) => DateOnly.FromDateTime(moment.ToOffset(Offset).DateTime);
}
