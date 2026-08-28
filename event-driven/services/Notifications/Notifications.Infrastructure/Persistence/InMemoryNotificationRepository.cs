using Notifications.Application.Ports;
using Notifications.Domain;

namespace Notifications.Infrastructure.Persistence;

/// <summary>
/// Dedup by idempotency key is enforced directly here under a lock, unlike event-sourcing/'s
/// repository, which relies on a Postgres unique index and translates a 23505 violation into
/// DuplicateNotificationException — there is no database left to enforce it in this architecture.
///
/// Registered as a singleton (see ServiceCollectionExtensions), not scoped: this dictionary IS
/// the storage, so a fresh instance per request would silently lose every notification between
/// calls. Notification is a mutable class, so mutating one obtained from FindAsync/FindDueAsync
/// already mutates what's stored here by reference — SaveAsync's second/third calls in
/// NotificationSenderWorker exist for symmetry with the port's contract, not because a separate
/// commit step is needed.
/// </summary>
public sealed class InMemoryNotificationRepository : INotificationRepository
{
    private readonly object _gate = new();
    private readonly Dictionary<Guid, Notification> _byId = [];
    private readonly Dictionary<string, Guid> _byIdempotencyKey = [];

    public Task<Notification?> FindAsync(Guid id, CancellationToken ct)
    {
        lock (_gate)
            return Task.FromResult(_byId.GetValueOrDefault(id));
    }

    public Task SaveAsync(Notification notification, CancellationToken ct)
    {
        lock (_gate)
        {
            if (_byIdempotencyKey.TryGetValue(notification.IdempotencyKey, out var existingId) && existingId != notification.Id)
                throw new DuplicateNotificationException(notification.IdempotencyKey);

            _byId[notification.Id] = notification;
            _byIdempotencyKey[notification.IdempotencyKey] = notification.Id;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Notification>> FindDueAsync(DateTimeOffset asOf, int limit, CancellationToken ct)
    {
        lock (_gate)
        {
            IReadOnlyList<Notification> due = _byId.Values
                .Where(n => n.Status == NotificationStatus.Scheduled && n.ScheduledFor <= asOf)
                .OrderBy(n => n.ScheduledFor)
                .Take(limit)
                .ToArray();

            return Task.FromResult(due);
        }
    }
}
