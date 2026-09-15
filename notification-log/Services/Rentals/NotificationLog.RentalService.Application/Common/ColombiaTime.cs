using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Common;

public static class ColombiaTime
{
    public static DateOnly Today(DateTimeOffset now)
        => DateOnly.FromDateTime(now.ToOffset(VisitPolicy.ColombiaOffset).Date);
}
