using Users.Domain;

namespace Users.Application.Ports;

/// <summary>
/// No EmailReservationChange parameter, unlike event-sourcing/'s IUserRepository: there is no
/// separate reservation document to release/reserve in this architecture. The implementation
/// (Users.Infrastructure's InMemoryUserRepository) derives the email-ownership check itself by
/// comparing the given user's VerifiedEmail before and after — see its own doc comment.
/// </summary>
public interface IUserRepository
{
    Task<User?> LoadAsync(Guid userId, CancellationToken ct);

    /// <summary>Publishes the stream-starting event and adopts <paramref name="user"/> — already
    /// built by the caller via <c>User.Create(registered)</c> — as this id's canonical state.</summary>
    Task StartAsync(User user, UserRegistered registered, CancellationToken ct);

    /// <summary>
    /// Publishes <paramref name="newEvents"/> and adopts <paramref name="user"/> — already mutated
    /// by the caller via <c>User.Apply</c> for each of them — as this id's canonical state. Throws
    /// <see cref="EmailAlreadyInUseException"/> without publishing anything if
    /// <paramref name="user"/>'s VerifiedEmail changed to one already owned by a different user.
    /// </summary>
    Task AppendAsync(Guid userId, User user, IReadOnlyList<object> newEvents, CancellationToken ct);
}
