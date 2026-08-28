using Marten;
using Notifications.Application.Ports;
using Notifications.Domain;

namespace Notifications.Infrastructure.Persistence;

/// <summary>Plain document storage — the replica is deliberately not event sourced (SPEC.md §8).</summary>
public sealed class UserContactRepository(IDocumentSession session) : IUserContactRepository
{
    public async Task<UserContact?> FindAsync(Guid userId, CancellationToken ct) =>
        await session.LoadAsync<UserContact>(userId, ct);

    public async Task SaveAsync(UserContact contact, CancellationToken ct)
    {
        session.Store(contact);
        await session.SaveChangesAsync(ct);
    }

    public async Task<int> CountAsync(CancellationToken ct) =>
        await session.Query<UserContact>().CountAsync(ct);
}
