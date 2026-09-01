using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Services.Dispatch;

public interface IWhatsAppNotificationSender
{
    Task<NotificationSendResult> SendAsync(string destination, RenderedMessage message, CancellationToken ct);
}
