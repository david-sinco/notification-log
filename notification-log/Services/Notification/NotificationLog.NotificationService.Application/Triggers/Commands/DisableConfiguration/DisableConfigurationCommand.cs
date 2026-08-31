namespace NotificationLog.NotificationService.Application.Triggers.Commands.DisableConfiguration;

public sealed record DisableConfigurationCommand(Guid TriggerId, Guid ConfigurationId);
