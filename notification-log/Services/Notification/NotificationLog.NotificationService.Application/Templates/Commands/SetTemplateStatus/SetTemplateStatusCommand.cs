namespace NotificationLog.NotificationService.Application.Templates.Commands.SetTemplateStatus;

public sealed record SetTemplateStatusCommand(Guid Id, bool IsEnabled);