namespace NotificationLog.NotificationService.Application.Templates.Dtos;

public sealed record TemplateDto(
    Guid Id,
    string Name,
    string Channel,
    string? Subject,
    string Body,
    bool IsEnabled);