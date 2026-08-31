namespace NotificationLog.NotificationService.Application.Triggers.Commands.ChangeConfigurationTemplate;

public sealed record ChangeConfigurationTemplateCommand(
    Guid TriggerId, Guid ConfigurationId, Guid TemplateId);
