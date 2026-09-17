namespace NotificationLog.NotificationService.Application.Notifications.Dtos;

public sealed record NotificationDto(
    Guid Id,
    Guid EventId,
    string EventKey,
    Guid? ConfigurationId,
    Guid TemplateId,
    Guid TemplateVersionId,
    Guid? RecipientId,
    string Channel,
    string Destination,
    string Status,
    string? ProviderMessageId,
    string? Error,
    DateTime OccurredAt,
    IReadOnlyDictionary<string, string>? Payload);
