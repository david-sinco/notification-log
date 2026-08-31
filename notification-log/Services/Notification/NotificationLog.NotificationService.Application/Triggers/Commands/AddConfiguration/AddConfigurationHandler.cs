using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Templates;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.AddConfiguration;

public sealed class AddConfigurationHandler
{
    private readonly INotificationTriggerRepository _triggers;
    private readonly INotificationTemplateRepository _templates;
    private readonly IUnitOfWork _uow;

    public AddConfigurationHandler(
        INotificationTriggerRepository triggers,
        INotificationTemplateRepository templates,
        IUnitOfWork uow)
        => (_triggers, _templates, _uow) = (triggers, templates, uow);

    public async Task<NotificationConfiguration> HandleAsync(AddConfigurationCommand cmd, CancellationToken ct)
    {
        var trigger = await _triggers.GetByIdAsync(cmd.TriggerId, ct)
            ?? throw new NotFoundException(nameof(NotificationTrigger), cmd.TriggerId);

        var template = await _templates.GetByIdAsync(cmd.TemplateId, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), cmd.TemplateId);

        if (!template.IsEnabled)
            throw new AppValidationException(
                $"La plantilla '{template.Name}' está desactivada.");

        if (template.Channel != cmd.Channel)
            throw new AppValidationException(
                $"La plantilla es de canal {template.Channel}, no {cmd.Channel}.");

        var configuration = trigger.AddConfiguration(template.Id, cmd.Channel);

        await _uow.SaveChangesAsync(ct);

        return configuration;
    }
}