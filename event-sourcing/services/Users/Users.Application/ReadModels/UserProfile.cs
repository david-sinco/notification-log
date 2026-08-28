using Users.Domain;

namespace Users.Application.ReadModels;

/// <summary>
/// Single-stream, inline projection — the cheap baseline, read-after-write consistent
/// (SPEC.md §4). Self-aggregating via public Apply(event) methods, same convention as the
/// User aggregate itself, so Marten can build it with zero Marten reference from this project.
/// Users.Infrastructure registers it as <c>Snapshot&lt;UserProfile&gt;(SnapshotLifecycle.Inline)</c>.
///
/// Setters are public: Marten's inline snapshot round-trips through JSON on every LoadAsync, and
/// a private setter silently deserializes to the property's default instead of failing loudly.
/// Mutate only via Apply by convention, not by the compiler.
/// </summary>
public sealed class UserProfile
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }
    public string? Phone { get; set; }
    public bool PhoneVerified { get; set; }
    public bool Active { get; set; }
    public NotificationPreferences Preferences { get; set; } = NotificationPreferences.Default;
    public int Version { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Required by Marten's self-aggregating convention for the event that starts a stream — a
    /// bare constructor + Apply(UserRegistered) is silently never invoked.
    /// </summary>
    public static UserProfile Create(UserRegistered e)
    {
        var profile = new UserProfile();
        profile.Apply(e);
        return profile;
    }

    public void Apply(UserRegistered e)
    {
        Id = e.UserId;
        Name = e.Name;
        Active = true;
        Preferences = NotificationPreferences.Default;
        Touch(e.OccurredAt);
    }

    public void Apply(EmailVerified e)
    {
        Email = e.Email;
        EmailVerified = true;
        Touch(e.OccurredAt);
    }

    public void Apply(PhoneVerified e)
    {
        Phone = e.Phone;
        PhoneVerified = true;
        Touch(e.OccurredAt);
    }

    public void Apply(PreferencesChanged e)
    {
        Preferences = e.Preferences;
        Touch(e.OccurredAt);
    }

    public void Apply(UserDeactivated e)
    {
        Active = false;
        Touch(e.OccurredAt);
    }

    public void Apply(UserReactivated e)
    {
        Active = true;
        Touch(e.OccurredAt);
    }

    // EmailChangeRequested/PhoneChangeRequested and the *VerificationFailed events don't change
    // anything this read model exposes, so there's deliberately no Apply for them — the pending
    // side of the state machine isn't part of this projection's shape.

    private void Touch(DateTimeOffset occurredAt)
    {
        Version++;
        UpdatedAt = occurredAt;
    }
}
