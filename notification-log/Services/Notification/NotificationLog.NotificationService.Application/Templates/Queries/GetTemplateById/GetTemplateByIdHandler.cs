using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Templates.Dtos;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Queries.GetTemplateById;

public sealed class GetTemplateByIdHandler
{
    private readonly INotificationTemplateRepository _templates;

    public GetTemplateByIdHandler(INotificationTemplateRepository templates)
        => _templates = templates;

    public async Task<TemplateDto> HandleAsync(GetTemplateByIdQuery query, CancellationToken ct)
    {
        var t = await _templates.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), query.Id);

        return new TemplateDto(
            t.Id,
            t.Name.Value,
            t.Channel.ToString(),
            t.IsEnabled,
            Map(t.CurrentVersion),
            t.Versions.OrderByDescending(v => v.Number).Select(Map).ToList());
    }

    private static TemplateVersionDto Map(TemplateVersion v)
        => new(v.Id, v.Number, v.Subject, v.Body, v.IsCurrent, v.CreatedAt);
}
