namespace NotificationLog.NotificationService.Application.Templates.Commands.UpdateTemplateContent;

public sealed record UpdateTemplateContentCommand(Guid Id, string? Subject, string Body);