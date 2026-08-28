namespace Users.Domain;

// Domain events. Exactly one of these ever crosses the service boundary in translated form
// (see Users.Application's integration-event mapper) — these types themselves never leave
// this service. SPEC.md §5.

public sealed record UserRegistered(
    Guid UserId, string Name, DateTimeOffset OccurredAt);

public sealed record EmailChangeRequested(
    Guid UserId, string Email, string TokenHash, DateTimeOffset ExpiresAt, DateTimeOffset OccurredAt);

public sealed record EmailVerified(
    Guid UserId, string Email, DateTimeOffset OccurredAt);

public sealed record EmailVerificationFailed(
    Guid UserId, VerificationFailureReason Reason, int FailedAttempts, DateTimeOffset OccurredAt);

public sealed record PhoneChangeRequested(
    Guid UserId, string Phone, string TokenHash, DateTimeOffset ExpiresAt, DateTimeOffset OccurredAt);

public sealed record PhoneVerified(
    Guid UserId, string Phone, DateTimeOffset OccurredAt);

public sealed record PhoneVerificationFailed(
    Guid UserId, VerificationFailureReason Reason, int FailedAttempts, DateTimeOffset OccurredAt);

public sealed record PreferencesChanged(
    Guid UserId, NotificationPreferences Preferences, DateTimeOffset OccurredAt);

public sealed record UserDeactivated(
    Guid UserId, DateTimeOffset OccurredAt);

public sealed record UserReactivated(
    Guid UserId, DateTimeOffset OccurredAt);
