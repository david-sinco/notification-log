using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Queries.ListTriggers;

public sealed class ListTriggersHandler(INotificationTriggerRepository triggers)
{
    private readonly INotificationTriggerRepository _triggers = triggers;

    public async Task<PagedResult<TriggerSummaryDto>> HandleAsync(ListTriggersQuery query, CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, total) = await _triggers.ListAsync(
            query.Search, query.IsEnabled, page, pageSize, ct);

        var dtos = items
            .Select(t => new TriggerSummaryDto(
                t.Id,
                t.EventKey.Value,
                t.Description,
                t.IsEnabled,
                t.Configurations.Count(c => c.IsEnabled)))
            .ToList();

        return new PagedResult<TriggerSummaryDto>(dtos, page, pageSize, total);
    }
}