namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed record PageOf<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
