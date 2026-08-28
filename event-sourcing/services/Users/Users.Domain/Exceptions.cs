namespace Users.Domain;

/// <summary>Base type for invariant violations raised by User.Decide* methods. Maps to HTTP 422.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>
/// Raised when the email-reservation document write hits a Postgres unique-key violation
/// (23505). Maps to HTTP 409 — see SPEC.md §4, "Cross-aggregate uniqueness".
/// </summary>
public sealed class EmailAlreadyInUseException : DomainException
{
    public EmailAlreadyInUseException(string email)
        : base($"Email '{email}' is already in use by another user.") { }
}

/// <summary>
/// Raised when Marten's optimistic-concurrency check fails on AppendOptimistic. Maps to HTTP 409.
/// </summary>
public sealed class ConcurrencyConflictException : DomainException
{
    public ConcurrencyConflictException(Guid userId)
        : base($"User '{userId}' was modified concurrently; reload and retry.") { }
}

/// <summary>Raised when a command targets a stream that doesn't exist. Maps to HTTP 404.</summary>
public sealed class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid userId) : base($"User '{userId}' was not found.") { }
}
