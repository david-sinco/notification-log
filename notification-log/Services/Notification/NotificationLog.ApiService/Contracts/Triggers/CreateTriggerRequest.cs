namespace NotificationLog.ApiService.Contracts.Triggers;

public sealed record CreateTriggerRequest(string EventKey, string Description);