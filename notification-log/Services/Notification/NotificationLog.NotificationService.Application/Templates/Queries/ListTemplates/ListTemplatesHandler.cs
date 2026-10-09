using Application.Shared.Pagination;
using NotificationLog.NotificationService.Application.Templates.Dtos;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Queries.ListTemplates;

public sealed class ListTemplatesHandler
{
    private readonly INotificationTemplateRepository _templates;

    public ListTemplatesHandler(INotificationTemplateRepository templates)
        => _templates = templates;

    public async Task<PagedResult<TemplateSummaryDto>> HandleAsync(
        ListTemplatesQuery query, CancellationToken ct)
    {
        var paging = new PageRequest(query.Page, query.PageSize);

        var (items, total) = await _templates.ListAsync(
            query.Search, query.Channel, query.IsEnabled, paging.Page, paging.PageSize, ct);

        var dtos = items
            .Select(t => new TemplateSummaryDto(
                t.Id, t.Name.Value, t.Channel.ToString(), t.CurrentVersion.Number, t.IsEnabled))
            .ToList();

        return new PagedResult<TemplateSummaryDto>(dtos, paging, total);
    }
}
