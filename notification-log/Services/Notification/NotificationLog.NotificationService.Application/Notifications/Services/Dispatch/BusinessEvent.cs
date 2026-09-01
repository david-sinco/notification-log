namespace NotificationLog.NotificationService.Application.Notifications.Services.Dispatch;

public sealed record BusinessEvent(
    Guid EventId,
    string EventKey,
    DateTime OccurredAt,
    Guid RecipientId,
    IReadOnlyDictionary<string, string> Data);
