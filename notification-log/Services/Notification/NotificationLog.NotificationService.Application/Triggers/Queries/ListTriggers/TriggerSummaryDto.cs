namespace NotificationLog.NotificationService.Application.Triggers.Queries.ListTriggers;

public sealed record TriggerSummaryDto(
    Guid Id,
    string EventKey,
    string Description,
    bool IsEnabled,
    int ActiveConfigurations);