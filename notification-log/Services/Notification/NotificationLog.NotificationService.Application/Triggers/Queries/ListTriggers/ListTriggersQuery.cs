namespace NotificationLog.NotificationService.Application.Triggers.Queries.ListTriggers;

public sealed record ListTriggersQuery(
    string? Search = null,
    bool? IsEnabled = null,
    int Page = 1,
    int PageSize = 20);