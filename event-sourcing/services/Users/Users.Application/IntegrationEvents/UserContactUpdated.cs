namespace Users.Application.IntegrationEvents;

public static class IntegrationTopics
{
    public const string UsersContact = "users.contact";
}

/// <summary>
/// Event-carried state transfer: the full current contact snapshot, not a delta (SPEC.md §5).
/// Idempotent, self-sufficient, and compactable by design — losing one message never leaves a
/// consumer's replica permanently wrong, because the next one carries everything again.
/// </summary>
public sealed record ContactPreferences(bool Email, bool Sms, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd);

public sealed record UserContactUpdated(
    Guid UserId,
    string Name,
    string? Email,
    string? Phone,
    bool EmailVerified,
    bool PhoneVerified,
    bool Active,
    ContactPreferences Preferences);

/// <summary>
/// The wire envelope (SPEC.md §5). Deliberately duplicated per service rather than shared —
/// see Shared/Contracts/schemas/README.md.
/// </summary>
public sealed record Envelope<T>(
    Guid EventId,
    string Type,
    Guid AggregateId,
    int Version,
    DateTimeOffset OccurredAt,
    int SchemaVersion,
    T Data);
