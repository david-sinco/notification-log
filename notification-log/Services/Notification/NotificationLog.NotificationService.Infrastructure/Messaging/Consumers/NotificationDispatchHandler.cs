using NotificationLog.Contracts.Notifications;
using NotificationLog.NotificationService.Application.Notifications.Services.Dispatch;

namespace NotificationLog.NotificationService.Infrastructure.Messaging.Consumers;

public static class NotificationDispatchHandler
{
    public static Task Handle(
        NotificationDispatchRequested message, NotificationDispatchService dispatchService, CancellationToken ct)
    {
        var businessEvent = MapToBusinessEvent(message); // Protobuf -> el BusinessEvent de Application
        return dispatchService.DispatchAsync(businessEvent, ct);
    }

    private static BusinessEvent MapToBusinessEvent(NotificationDispatchRequested message) =>
        new(
            EventId: Guid.Parse(message.EventId),
            EventKey: message.EventKey,
            OccurredAt: message.OccurredAt.ToDateTime(),
            RecipientId: Guid.Parse(message.RecipientId),
            Data: message.Data.ToDictionary(kv => kv.Key, kv => kv.Value));
}
