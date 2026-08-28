using Users.Domain;

namespace Users.Application.Ports;

/// <summary>
/// Directs the one Marten-specific hard case (SPEC.md §4, "Cross-aggregate uniqueness") without
/// leaking Marten into Application: when a command's new events change which email is verified,
/// the handler says so explicitly here, and Users.Infrastructure's implementation is responsible
/// for releasing/reserving the reservation document in the same transaction as the event append.
/// </summary>
public sealed record EmailReservationChange(string? Release, string? Reserve);

public interface IUserRepository
{
    Task<User?> LoadAsync(Guid userId, CancellationToken ct);

    Task StartAsync(Guid userId, UserRegistered registered, CancellationToken ct);

    Task AppendAsync(Guid userId, IReadOnlyList<object> newEvents, EmailReservationChange? reservationChange, CancellationToken ct);
}
