using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Queries.ListNotifications;

public sealed record ListNotificationsQuery(
    Guid? RecipientId = null,
    string? EventKey = null,
    DeliveryStatus? Status = null,
    int Page = 1,
    int PageSize = 20);
