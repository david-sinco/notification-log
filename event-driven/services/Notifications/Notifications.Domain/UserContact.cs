namespace Notifications.Domain;

public enum NotificationChannel { Email, Sms }

/// <summary>
/// The replica — a plain document, not an aggregate, holding only the fields this service uses.
/// No Version field, unlike event-sourcing/'s UserContact: that field exists there so a concurrent
/// scoped snapshot-merge can drop a stale one (SPEC.md §8's whole trick). Here, ContactMaterializer
/// is a single sequential consumer with no concurrent merges to guard against — the same idea
/// (drop anything already applied) is instead enforced once, at the offset-watermark level, the
/// same mechanism Users.Infrastructure.Materializer.UserMaterializer uses on its own side of the
/// same log (see ContactMaterializer's own note).
///
/// Gains an Apply(event) overload per raw event type this service understands, applied directly
/// by ContactMaterializer — keeping the fold logic here as plain records/methods rather than ad
/// hoc code in Infrastructure, the same Decide/Apply-adjacent discipline Users.Domain uses.
/// </summary>
public sealed class UserContact
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool EmailVerified { get; set; }
    public bool PhoneVerified { get; set; }
    public bool Active { get; set; }
    public bool EmailOptIn { get; set; }
    public bool SmsOptIn { get; set; }
    public TimeOnly? QuietHoursStart { get; set; }
    public TimeOnly? QuietHoursEnd { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public bool CanReceive(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => Active && EmailOptIn && EmailVerified && !string.IsNullOrEmpty(Email),
        NotificationChannel.Sms => Active && SmsOptIn && PhoneVerified && !string.IsNullOrEmpty(Phone),
        _ => false,
    };

    /// <summary>Handles a window that wraps past midnight (e.g. 22:00-07:00).</summary>
    public bool IsQuietAt(TimeOnly localTime)
    {
        if (QuietHoursStart is not { } start || QuietHoursEnd is not { } end)
            return false;

        return start <= end
            ? localTime >= start && localTime < end
            : localTime >= start || localTime < end;
    }

    /// <summary>
    /// Delays into the future rather than dropping. Lab simplification: treats the instant's UTC
    /// wall-clock time as "local" — there's no per-user timezone in this model.
    /// </summary>
    public DateTimeOffset NextSendableMoment(DateTimeOffset from)
    {
        if (QuietHoursStart is not { } start || QuietHoursEnd is not { } end)
            return from;

        var localTime = TimeOnly.FromDateTime(from.UtcDateTime);
        if (!IsQuietAt(localTime))
            return from;

        var candidate = new DateTimeOffset(from.UtcDateTime.Date.Add(end.ToTimeSpan()), TimeSpan.Zero);
        return candidate <= from ? candidate.AddDays(1) : candidate;
    }

    public static UserContact FromRegistration(UserRegisteredEvent e) => new()
    {
        Id = e.UserId,
        Name = e.Name,
        Active = true,
        EmailOptIn = true, // mirrors NotificationPreferences.Default in Users.Domain
        UpdatedAt = e.OccurredAt,
    };

    public void Apply(EmailVerifiedEvent e)
    {
        Email = e.Email;
        EmailVerified = true;
        UpdatedAt = e.OccurredAt;
    }

    public void Apply(PhoneVerifiedEvent e)
    {
        Phone = e.Phone;
        PhoneVerified = true;
        UpdatedAt = e.OccurredAt;
    }

    public void Apply(PreferencesChangedEvent e)
    {
        EmailOptIn = e.Preferences.EmailEnabled;
        SmsOptIn = e.Preferences.SmsEnabled;
        QuietHoursStart = e.Preferences.QuietHoursStart;
        QuietHoursEnd = e.Preferences.QuietHoursEnd;
        UpdatedAt = e.OccurredAt;
    }

    public void Apply(UserDeactivatedEvent e)
    {
        Active = false;
        UpdatedAt = e.OccurredAt;
    }

    public void Apply(UserReactivatedEvent e)
    {
        Active = true;
        UpdatedAt = e.OccurredAt;
    }
}
