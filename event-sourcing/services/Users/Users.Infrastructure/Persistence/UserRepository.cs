using Marten;
using Marten.Exceptions;
using Npgsql;
using Users.Application.Ports;
using Users.Domain;
using Users.Infrastructure.Outbox;

namespace Users.Infrastructure.Persistence;

/// <summary>
/// Implements Users.Application's IUserRepository port on top of Marten. Ties together the three
/// things that must commit atomically (SPEC.md §4 and §6): the event append, the email
/// reservation document, and the outbox rows drained from the request-scoped OutboxBuffer —
/// all on the one live session, flushed by a single SaveChangesAsync.
/// </summary>
public sealed class UserRepository(IDocumentSession session, OutboxBuffer outboxBuffer, TimeProvider clock)
    : IUserRepository
{
    public async Task<User?> LoadAsync(Guid userId, CancellationToken ct) =>
        await session.Events.FetchLatest<User>(userId, ct);

    public async Task StartAsync(Guid userId, UserRegistered registered, CancellationToken ct)
    {
        session.Events.StartStream<User>(userId, registered);
        await FlushOutboxAndSaveAsync(ct);
    }

    public async Task AppendAsync(
        Guid userId, IReadOnlyList<object> newEvents, EmailReservationChange? reservationChange, CancellationToken ct)
    {
        await session.Events.AppendOptimistic(userId, ct, newEvents.ToArray());

        if (reservationChange?.Release is { } release)
            session.Delete<EmailReservation>(EmailNormalizer.Normalize(release));

        if (reservationChange?.Reserve is { } reserve)
            session.Store(new EmailReservation { Id = EmailNormalizer.Normalize(reserve), UserId = userId });

        try
        {
            await FlushOutboxAndSaveAsync(ct);
        }
        catch (ConcurrentUpdateException)
        {
            throw new ConcurrencyConflictException(userId);
        }
        catch (Exception ex) when (FindPostgresException(ex) is { SqlState: "23505" })
        {
            throw new EmailAlreadyInUseException(reservationChange?.Reserve ?? string.Empty);
        }
    }

    private async Task FlushOutboxAndSaveAsync(CancellationToken ct)
    {
        foreach (var message in outboxBuffer.DrainAsMessages(clock.GetUtcNow()))
            session.Store(message);

        await session.SaveChangesAsync(ct);
    }

    /// <summary>Postgres wraps the driver exception at least once between Npgsql and Marten.</summary>
    private static PostgresException? FindPostgresException(Exception? ex)
    {
        for (; ex is not null; ex = ex.InnerException)
            if (ex is PostgresException postgres)
                return postgres;

        return null;
    }
}
