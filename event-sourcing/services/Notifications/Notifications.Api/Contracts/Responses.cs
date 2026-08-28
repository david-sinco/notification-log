using Notifications.Domain;

namespace Notifications.Api.Contracts;

public sealed record NotificationResponse(
    Guid Id, Guid UserId, NotificationChannel Channel, NotificationStatus Status,
    int Attempts, DateTimeOffset ScheduledFor, DateTimeOffset? SentAt, string? LastError)
{
    public static NotificationResponse From(Notification n) => new(
        n.Id, n.UserId, n.Channel, n.Status, n.Attempts, n.ScheduledFor, n.SentAt, n.LastError);
}

public sealed record ReplicaStatusResponse(int Count);
