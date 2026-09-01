using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Application.Notifications.Commands.RecordNotificationSuccess;

public sealed record RecordNotificationSuccessCommand(
    string EventKey,
    Guid ConfigurationId,
    Guid TemplateId,
    Guid TemplateVersionId,
    Guid RecipientId,
    NotificationChannel Channel,
    string Destination,
    string? ProviderMessageId,
    DateTime OccurredAt,
    IReadOnlyDictionary<string, string>? Payload = null);
