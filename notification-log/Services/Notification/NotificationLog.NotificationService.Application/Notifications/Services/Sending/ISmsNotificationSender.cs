using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Services.Sending;

public interface ISmsNotificationSender
{
    Task<NotificationSendResult> SendAsync(string destination, RenderedMessage message, CancellationToken ct);
}
