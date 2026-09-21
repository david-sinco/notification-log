using Application.Shared.Abstractions;
using Microsoft.EntityFrameworkCore;
using NotificationLog.IdentityService.Infrastructure.Messaging;

namespace NotificationLog.IdentityService.Infrastructure.Persistence;

internal sealed class OutboxUnitOfWork : IUnitOfWork
{
    private readonly IdentityServiceDbContext _db;
    private readonly IdentityOutbox _outbox;

    public OutboxUnitOfWork(IdentityServiceDbContext db, IdentityOutbox outbox)
        => (_db, _outbox) = (db, outbox);

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        var changes = _db.ChangeTracker
            .Entries()
            .Count(entry => entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted);

        await _outbox.SaveChangesAndFlushAsync(ct);

        return changes;
    }
}
