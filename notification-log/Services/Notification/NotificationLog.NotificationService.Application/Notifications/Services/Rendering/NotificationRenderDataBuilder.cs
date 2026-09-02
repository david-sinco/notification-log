using NotificationLog.NotificationService.Application.Notifications.Services.Dispatch;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Notifications.Services.Rendering;

internal static class NotificationRenderDataBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(BusinessEvent businessEvent, Recipient recipient)
    {
        var data = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase)
        {
            ["recipient.id"] = recipient.Id,
            ["recipient.name"] = recipient.Name,
            ["recipient.email"] = recipient.Email,
            ["recipient.phone"] = recipient.Phone,
            ["recipient.locale"] = recipient.Locale,
            ["recipient.timeZone"] = recipient.TimeZone
        };

        foreach (var (key, value) in recipient.Attributes)
            data[$"recipient.{key}"] = value;

        foreach (var (key, value) in businessEvent.Data)
            data[key] = value;

        return data;
    }
}
