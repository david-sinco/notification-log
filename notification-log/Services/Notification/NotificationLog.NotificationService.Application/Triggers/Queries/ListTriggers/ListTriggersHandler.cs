using Application.Shared.Pagination;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Queries.ListTriggers;

public sealed class ListTriggersHandler(INotificationTriggerRepository triggers)
{
    private readonly INotificationTriggerRepository _triggers = triggers;

    public async Task<PagedResult<TriggerSummaryDto>> HandleAsync(ListTriggersQuery query, CancellationToken ct)
    {
        var paging = new PageRequest(query.Page, query.PageSize);

        var (items, total) = await _triggers.ListAsync(
            query.Search, query.IsEnabled, paging.Page, paging.PageSize, ct);

        var dtos = items
            .Select(t => new TriggerSummaryDto(
                t.Id,
                t.EventKey.Value,
                t.Description,
                t.IsEnabled,
                t.Configurations.Count(c => c.IsEnabled),
                t.Configurations.Where(c => c.IsEnabled).Select(c => c.Channel.ToString()).ToList()))
            .ToList();

        return new PagedResult<TriggerSummaryDto>(dtos, paging, total);
    }
}