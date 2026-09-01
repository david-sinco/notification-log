namespace NotificationLog.NotificationService.Application.Templates.Commands.SetTemplateStatus;

public sealed record SetTemplateStatusCommand(Guid TemplateId, bool IsEnabled);
