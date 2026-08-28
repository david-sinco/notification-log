using NotificationLog.NotificationService.Application.Common;
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
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, total) = await _templates.ListAsync(
            query.Search, query.Channel, query.IsEnabled, page, pageSize, ct);

        var dtos = items
            .Select(t => new TemplateSummaryDto(
                t.Id, t.Name.Value, t.Channel.ToString(), t.Subject, t.IsEnabled))
            .ToList();

        return new PagedResult<TemplateSummaryDto>(dtos, page, pageSize, total);
    }
}