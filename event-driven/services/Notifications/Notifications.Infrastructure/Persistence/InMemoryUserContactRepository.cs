using Notifications.Application.Ports;
using Notifications.Domain;
using Notifications.Infrastructure.EventLog;

namespace Notifications.Infrastructure.Persistence;

/// <summary>Thin adapter over ContactMaterializer, which owns the actual state — see its own doc
/// comment. Waits for the initial replay to finish before answering, same as Users' equivalent.</summary>
public sealed class InMemoryUserContactRepository(ContactMaterializer materializer) : IUserContactRepository
{
    public async Task<UserContact?> FindAsync(Guid userId, CancellationToken ct)
    {
        await materializer.WaitUntilReadyAsync(ct);
        return materializer.TryGet(userId);
    }

    public async Task<int> CountAsync(CancellationToken ct)
    {
        await materializer.WaitUntilReadyAsync(ct);
        return materializer.Count();
    }
}
