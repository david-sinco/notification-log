namespace Notifications.Domain;

public enum NotificationChannel { Email, Sms }

/// <summary>
/// The replica (SPEC.md §8, piece 1) — a plain document, not an aggregate, holding only the
/// fields this service uses. Replicated read state wants last-write-wins keyed by version; this
/// type is deliberately not event sourced.
///
/// Idempotency is version-based and is the whole trick: a caller applying a snapshot must check
/// <c>snapshot.Version &lt;= existing.Version</c> and drop it if so (see
/// Notifications.Infrastructure's consumer). That single check makes redelivery, out-of-order
/// arrival, and a full replay from offset 0 all safe.
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
    public int Version { get; set; }
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
}
