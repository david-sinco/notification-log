namespace NotificationLog.NotificationService.Application.Templates.Commands.PublishTemplateVersion;

public sealed record PublishTemplateVersionCommand(Guid TemplateId, string? Subject, string Body);
