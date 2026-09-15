namespace NotificationLog.RentalService.Application.Common;

public static class Paging
{
    public static int Page(int page) => Math.Max(page, 1);

    public static int Size(int pageSize) => Math.Clamp(pageSize, 1, 100);
}
