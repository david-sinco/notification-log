using Notifications.Domain;

namespace Notifications.Api.Contracts;

public sealed record RequestNotificationRequest(
    Guid UserId, NotificationChannel Channel, string IdempotencyKey, string Body);
