namespace NotificationLog.Web.Api.Notifications;

// Espejo de NotificationLog.NotificationService.Domain.Notifications.DeliveryStatus.
public enum DeliveryStatus
{
    Sent = 1,
    Failed = 2
}

public sealed record NotificationDto(
    Guid Id,
    Guid EventId,
    string EventKey,
    Guid ConfigurationId,
    Guid TemplateId,
    Guid TemplateVersionId,
    Guid RecipientId,
    string Channel,
    string Destination,
    string Status,
    string? ProviderMessageId,
    string? Error,
    DateTime OccurredAt,
    IReadOnlyDictionary<string, string> Payload);
