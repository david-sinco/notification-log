namespace NotificationLog.NotificationService.Application.Triggers.Dtos;

public sealed record TriggerDto(
    Guid Id,
    string EventKey,
    string Description,
    bool IsEnabled,
    IReadOnlyList<ConfigurationDto> Configurations);