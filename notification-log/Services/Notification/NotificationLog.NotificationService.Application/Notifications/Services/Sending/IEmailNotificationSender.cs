using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Services.Sending;

public interface IEmailNotificationSender
{
    Task<NotificationSendResult> SendAsync(string destination, RenderedMessage message, CancellationToken ct);
}
