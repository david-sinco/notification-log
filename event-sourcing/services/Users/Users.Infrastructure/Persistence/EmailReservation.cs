namespace Users.Infrastructure.Persistence;

/// <summary>
/// Enforces email uniqueness across all users (SPEC.md §4, invariant 7). Id *is* the normalized
/// email, so Postgres's primary-key constraint does the enforcing — a second user verifying the
/// same email hits a 23505 unique violation when this document is stored in the same transaction
/// as the EmailVerified event append.
/// </summary>
public sealed class EmailReservation
{
    public string Id { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}
