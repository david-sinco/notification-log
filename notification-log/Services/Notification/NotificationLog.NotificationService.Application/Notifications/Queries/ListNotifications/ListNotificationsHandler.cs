using Application.Shared.Pagination;
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
        var paging = new PageRequest(query.Page, query.PageSize);

        var (items, total) = await _notifications.ListAsync(
            query.RecipientId, query.EventKey, query.Status, query.Channel, query.Search, paging.Page, paging.PageSize, ct);

        var dtos = items
            .Select(n => new NotificationDto(
                n.Id,
                n.EventId,
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
                n.OccurredAt,
                n.Payload))
            .ToList();

        return new PagedResult<NotificationDto>(dtos, paging, total);
    }
}
