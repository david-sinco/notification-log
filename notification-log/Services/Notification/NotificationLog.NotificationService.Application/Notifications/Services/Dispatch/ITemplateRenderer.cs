using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Services.Dispatch;

public interface ITemplateRenderer
{
    RenderedMessage Render(string? subjectTemplate, string bodyTemplate, IReadOnlyDictionary<string, object?> data);
}
