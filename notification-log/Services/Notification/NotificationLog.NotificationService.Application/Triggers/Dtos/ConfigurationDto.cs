namespace NotificationLog.NotificationService.Application.Triggers.Dtos;

public sealed record ConfigurationDto(
    Guid Id,
    Guid TemplateId,
    string Channel,
    bool IsEnabled);