namespace NotificationLog.NotificationService.Application.Templates.Dtos;

public sealed record TemplateDto(
    Guid Id,
    string Name,
    string Channel,
    bool IsEnabled,
    TemplateVersionDto CurrentVersion,
    IReadOnlyList<TemplateVersionDto> History);