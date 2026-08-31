using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Templates;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.ChangeConfigurationTemplate;

public sealed class ChangeConfigurationTemplateHandler
{
    private readonly INotificationTriggerRepository _triggers;
    private readonly INotificationTemplateRepository _templates;
    private readonly IUnitOfWork _uow;

    public ChangeConfigurationTemplateHandler(
        INotificationTriggerRepository triggers,
        INotificationTemplateRepository templates,
        IUnitOfWork uow)
        => (_triggers, _templates, _uow) = (triggers, templates, uow);

    public async Task HandleAsync(ChangeConfigurationTemplateCommand cmd, CancellationToken ct)
    {
        var trigger = await _triggers.GetByIdAsync(cmd.TriggerId, ct)
            ?? throw new NotFoundException(nameof(NotificationTrigger), cmd.TriggerId);

        var template = await _templates.GetByIdAsync(cmd.TemplateId, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), cmd.TemplateId);

        if (!template.IsEnabled)
            throw new AppValidationException(
                $"La plantilla '{template.Name}' está desactivada.");

        trigger.ChangeConfigurationTemplate(cmd.ConfigurationId, template.Id, template.Channel);

        await _uow.SaveChangesAsync(ct);
    }
}
