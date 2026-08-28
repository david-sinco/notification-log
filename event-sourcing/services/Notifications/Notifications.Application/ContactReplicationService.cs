using Notifications.Application.Ports;
using Notifications.Domain;

namespace Notifications.Application;

/// <summary>
/// Applies an incoming UserContactUpdated snapshot. The version check is the whole trick
/// (SPEC.md §8): it makes redelivery, out-of-order arrival, and a full replay from offset 0 all
/// safe. Skipping it corrupts the replica and makes the Kafka bootstrap/reprocess experiments
/// meaningless.
/// </summary>
public sealed class ContactReplicationService(IUserContactRepository contacts)
{
    public async Task ApplySnapshotAsync(UserContact snapshot, CancellationToken ct)
    {
        // The check-then-write below is only safe if applies for the same user never overlap —
        // see UserContactLocks for why that isn't guaranteed by the transport alone.
        using var _ = await UserContactLocks.AcquireAsync(snapshot.Id, ct);

        var existing = await contacts.FindAsync(snapshot.Id, ct);
        if (existing is not null && snapshot.Version <= existing.Version)
            return; // stale, drop

        await contacts.SaveAsync(snapshot, ct);
    }
}
