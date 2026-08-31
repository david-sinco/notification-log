namespace NotificationLog.NotificationService.Application.Triggers.Commands.EnableConfiguration;

public sealed record EnableConfigurationCommand(Guid TriggerId, Guid ConfigurationId);
