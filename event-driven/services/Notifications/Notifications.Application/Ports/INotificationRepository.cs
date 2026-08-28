using Notifications.Domain;

namespace Notifications.Application.Ports;

public interface INotificationRepository
{
    Task<Notification?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>Throws DuplicateNotificationException if the idempotency key is already in use —
    /// enforced by the implementation itself here, not by a database unique index (there is none
    /// in this architecture).</summary>
    Task SaveAsync(Notification notification, CancellationToken ct);

    /// <summary>Scheduled notifications whose ScheduledFor has arrived, for the sender worker.</summary>
    Task<IReadOnlyList<Notification>> FindDueAsync(DateTimeOffset asOf, int limit, CancellationToken ct);
}
