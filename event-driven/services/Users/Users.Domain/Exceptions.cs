namespace Users.Domain;

/// <summary>Base type for invariant violations raised by User.Decide* methods. Maps to HTTP 422.</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}

/// <summary>
/// Raised when the in-memory email-ownership index (folded from users.events, not a database
/// constraint — see Users.Infrastructure.Materializer.UserMaterializer) already maps this email to
/// a different user. Maps to HTTP 409.
/// </summary>
public sealed class EmailAlreadyInUseException : DomainException
{
    public EmailAlreadyInUseException(string email)
        : base($"Email '{email}' is already in use by another user.") { }
}

/// <summary>Raised when a command targets a user id nothing has ever been produced for. Maps to HTTP 404.</summary>
public sealed class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid userId) : base($"User '{userId}' was not found.") { }
}

// No ConcurrencyConflictException here, unlike event-sourcing/'s Users.Domain: that type exists to
// surface Marten's optimistic-concurrency check on AppendOptimistic, i.e. two concurrent writers
// racing on the same aggregate. This architecture has exactly one writer for the whole log (see
// UserMaterializer's single global lock) by construction, so the failure mode it reports can never
// occur here — porting the type would just be dead plumbing with nothing left to throw it.
