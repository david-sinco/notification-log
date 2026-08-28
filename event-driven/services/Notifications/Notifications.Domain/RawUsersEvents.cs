namespace Notifications.Domain;

// The subset of Users' raw users.events this service understands, duplicated here rather than
// referenced from Users.Domain — the same "no shared DTO assembly" principle event-sourcing/'s
// Contracts uses for its one translated event, now applied to a raw multi-type log instead
// (Shared/Contracts/schemas/README.md). Property names and shapes must match what
// Users.Infrastructure.EventLog.UsersEventCodec actually serializes field-for-field — that
// coupling to the producer's wire shape, instead of a translated boundary event, is the accepted
// cost of this architecture (event-driven/README.md), not an oversight.
//
// Four of the ten types on the wire have no record here at all — EmailChangeRequested,
// PhoneChangeRequested, EmailVerificationFailed, PhoneVerificationFailed — because this replica
// only cares about verified state; ContactMaterializer recognizes their type tags and skips them
// without needing a record to deserialize into.

public sealed record UserRegisteredEvent(Guid UserId, string Name, DateTimeOffset OccurredAt);

public sealed record EmailVerifiedEvent(Guid UserId, string Email, DateTimeOffset OccurredAt);

public sealed record PhoneVerifiedEvent(Guid UserId, string Phone, DateTimeOffset OccurredAt);

public sealed record ContactPreferencesEvent(bool EmailEnabled, bool SmsEnabled, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd);

public sealed record PreferencesChangedEvent(Guid UserId, ContactPreferencesEvent Preferences, DateTimeOffset OccurredAt);

public sealed record UserDeactivatedEvent(Guid UserId, DateTimeOffset OccurredAt);

public sealed record UserReactivatedEvent(Guid UserId, DateTimeOffset OccurredAt);
