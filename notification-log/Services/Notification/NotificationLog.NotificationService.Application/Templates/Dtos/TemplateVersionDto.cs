namespace NotificationLog.NotificationService.Application.Templates.Dtos;

public sealed record TemplateVersionDto(Guid Id, int Number, string? Subject, string Body, bool IsCurrent, DateTime CreatedAt);