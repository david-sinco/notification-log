using Marten;
using Npgsql;
using Notifications.Application.Ports;
using Notifications.Domain;

namespace Notifications.Infrastructure.Persistence;

/// <summary>
/// Dedup by idempotency key is enforced with a Marten unique index (registered in
/// ServiceCollectionExtensions) — SaveAsync translates the resulting Postgres 23505 into
/// DuplicateNotificationException (SPEC.md §8).
/// </summary>
public sealed class NotificationRepository(IDocumentSession session) : INotificationRepository
{
    public async Task<Notification?> FindAsync(Guid id, CancellationToken ct) =>
        await session.LoadAsync<Notification>(id, ct);

    public async Task SaveAsync(Notification notification, CancellationToken ct)
    {
        session.Store(notification);

        try
        {
            await session.SaveChangesAsync(ct);
        }
        catch (Exception ex) when (FindPostgresException(ex) is { SqlState: "23505" })
        {
            throw new DuplicateNotificationException(notification.IdempotencyKey);
        }
    }

    public async Task<IReadOnlyList<Notification>> FindDueAsync(DateTimeOffset asOf, int limit, CancellationToken ct) =>
        await session.Query<Notification>()
            .Where(n => n.Status == NotificationStatus.Scheduled && n.ScheduledFor <= asOf)
            .OrderBy(n => n.ScheduledFor)
            .Take(limit)
            .ToListAsync(ct);

    private static PostgresException? FindPostgresException(Exception? ex)
    {
        for (; ex is not null; ex = ex.InnerException)
            if (ex is PostgresException postgres)
                return postgres;

        return null;
    }
}
