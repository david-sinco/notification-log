using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Triggers.Dtos;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Queries.GetTriggerById;

public sealed class GetTriggerByIdHandler(INotificationTriggerRepository triggers)
{
    private readonly INotificationTriggerRepository _triggers = triggers;

    public async Task<TriggerDto> HandleAsync(GetTriggerByIdQuery query, CancellationToken ct)
    {
        var trigger = await _triggers.GetByIdAsync(query.Id, ct)
            ?? throw new NotFoundException(nameof(NotificationTrigger), query.Id);

        return new TriggerDto(
            trigger.Id,
            trigger.EventKey.Value,
            trigger.Description,
            trigger.IsEnabled,
            [.. trigger.Configurations
                .Select(c => new ConfigurationDto(
                    c.Id,
                    c.TemplateId,
                    c.Channel.ToString(),
                    c.IsEnabled))]);
    }
}