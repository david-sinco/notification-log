using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Application.Notifications.Commands.RecordNotificationFailure;

public sealed record RecordNotificationFailureCommand(
    Guid EventId,
    string EventKey,
    Guid ConfigurationId,
    Guid TemplateId,
    Guid TemplateVersionId,
    Guid RecipientId,
    NotificationChannel Channel,
    string Destination,
    string Error,
    DateTime OccurredAt,
    IReadOnlyDictionary<string, string>? Payload = null);
