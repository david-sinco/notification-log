using System.Collections.Concurrent;

namespace Notifications.Application;

/// <summary>
/// Serializes ContactReplicationService's read-check-write per user. Without this, two snapshots
/// for the same user arriving close together (e.g. RabbitMQ's async consumer dispatching the
/// next delivery before the previous handler's Task has completed) can both read the same "stale"
/// baseline, both decide they're the newer one, and race to write — the older one can win purely
/// because its database round-trip happened to finish last. The idempotency check in
/// ContactReplicationService is only correct if applies for one user never overlap; this is a
/// process-local guard for that, not a distributed lock (fine for this lab's single Notifications
/// instance — a real multi-instance deployment would need a database-level guard instead).
/// </summary>
internal static class UserContactLocks
{
    private static readonly ConcurrentDictionary<Guid, SemaphoreSlim> Locks = new();

    public static async Task<IDisposable> AcquireAsync(Guid userId, CancellationToken ct)
    {
        var semaphore = Locks.GetOrAdd(userId, static _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(ct);
        return new Releaser(semaphore);
    }

    private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
    {
        public void Dispose() => semaphore.Release();
    }
}
