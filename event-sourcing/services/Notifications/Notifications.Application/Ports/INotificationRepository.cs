using Notifications.Domain;

namespace Notifications.Application.Ports;

public interface INotificationRepository
{
    Task<Notification?> FindAsync(Guid id, CancellationToken ct);

    Task SaveAsync(Notification notification, CancellationToken ct);

    /// <summary>Scheduled notifications whose ScheduledFor has arrived, for the sender worker.</summary>
    Task<IReadOnlyList<Notification>> FindDueAsync(DateTimeOffset asOf, int limit, CancellationToken ct);
}
