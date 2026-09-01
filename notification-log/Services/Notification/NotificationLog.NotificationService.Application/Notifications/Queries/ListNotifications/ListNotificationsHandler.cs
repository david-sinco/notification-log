using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Notifications.Dtos;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Queries.ListNotifications;

public sealed class ListNotificationsHandler
{
    private readonly INotificationRepository _notifications;

    public ListNotificationsHandler(INotificationRepository notifications)
        => _notifications = notifications;

    public async Task<PagedResult<NotificationDto>> HandleAsync(
        ListNotificationsQuery query, CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, total) = await _notifications.ListAsync(
            query.RecipientId, query.EventKey, query.Status, page, pageSize, ct);

        var dtos = items
            .Select(n => new NotificationDto(
                n.Id,
                n.EventKey,
                n.ConfigurationId,
                n.TemplateId,
                n.TemplateVersionId,
                n.RecipientId,
                n.Channel.ToString(),
                n.Destination,
                n.Status.ToString(),
                n.ProviderMessageId,
                n.Error,
                n.OccurredAt))
            .ToList();

        return new PagedResult<NotificationDto>(dtos, page, pageSize, total);
    }
}
