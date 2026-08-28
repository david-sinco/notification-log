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
        var template = await _templates.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), query.Id);

        return new TemplateDto(
            template.Id,
            template.Name.Value,
            template.Channel.ToString(),
            template.Subject,
            template.Body,
            template.IsEnabled);
    }
}