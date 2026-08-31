using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.AddConfiguration;

public sealed record AddConfigurationCommand(
    Guid TriggerId, Guid TemplateId, NotificationChannel Channel);