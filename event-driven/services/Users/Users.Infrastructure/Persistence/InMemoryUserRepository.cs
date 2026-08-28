using Users.Application.Ports;
using Users.Domain;
using Users.Infrastructure.Materializer;

namespace Users.Infrastructure.Persistence;

/// <summary>
/// Thin adapter over UserMaterializer, which owns the actual state, the single-writer lock, and
/// the produce+apply mechanics — this class exists only so Users.Application depends on the
/// IUserRepository port, not on Users.Infrastructure directly. Every method waits for the initial
/// replay to finish first, so a request arriving during boot blocks rather than seeing a
/// half-replayed, wrong answer.
/// </summary>
public sealed class InMemoryUserRepository(UserMaterializer materializer) : IUserRepository
{
    public async Task<User?> LoadAsync(Guid userId, CancellationToken ct)
    {
        await materializer.WaitUntilReadyAsync(ct);
        return materializer.TryGet(userId);
    }

    public async Task StartAsync(User user, UserRegistered registered, CancellationToken ct)
    {
        await materializer.WaitUntilReadyAsync(ct);
        await materializer.StartAsync(user, registered, ct);
    }

    public async Task AppendAsync(Guid userId, User user, IReadOnlyList<object> newEvents, CancellationToken ct)
    {
        await materializer.WaitUntilReadyAsync(ct);
        await materializer.AppendAsync(userId, user, newEvents, ct);
    }
}
