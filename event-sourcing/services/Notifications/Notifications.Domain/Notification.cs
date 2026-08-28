namespace Notifications.Domain;

public enum NotificationStatus { Scheduled, Sending, Sent, Failed, Discarded }

/// <summary>
/// The notification (SPEC.md §8, piece 2): a small entity with real rules and a state machine.
///
/// <code>
/// Scheduled -> Sending -> Sent
///                 |
///                 +-> Failed -> Scheduled (retry, exponential backoff)
///                 |
///                 +-> Discarded  (permanent error, or attempts exhausted)
/// </code>
///
/// Deliberately not event sourced, same as UserContact — this service replicates and reacts,
/// it doesn't own a stream of its own history.
/// </summary>
public sealed class Notification
{
    public const int MaxAttempts = 5;

    private static readonly TimeSpan[] Backoff =
    [
        TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(8),
        TimeSpan.FromSeconds(16), TimeSpan.FromSeconds(32),
    ];

    // Public, not private, setters: this document round-trips through JSON on every
    // LoadAsync/Store — Marten's default serializer silently deserializes a private setter to
    // the property's default instead of failing loudly (same reasoning as Users.Domain.User).
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public NotificationChannel Channel { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public NotificationStatus Status { get; set; }
    public int Attempts { get; set; }
    public DateTimeOffset ScheduledFor { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? LastError { get; set; }

    /// <summary>
    /// Schedules a notification. Callers must have already checked <see cref="UserContact.CanReceive"/>
    /// and computed <paramref name="sendAt"/> via <see cref="UserContact.NextSendableMoment"/> —
    /// this constructor doesn't re-derive either, so it stays a pure state-machine transition.
    /// </summary>
    public static Notification Schedule(
        Guid userId, NotificationChannel channel, string idempotencyKey, string body, DateTimeOffset sendAt) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Channel = channel,
        IdempotencyKey = idempotencyKey,
        Body = body,
        Status = NotificationStatus.Scheduled,
        ScheduledFor = sendAt,
    };

    public void BeginSending()
    {
        EnsureStatus(NotificationStatus.Scheduled, nameof(BeginSending));
        Status = NotificationStatus.Sending;
    }

    public void MarkSent(DateTimeOffset now)
    {
        EnsureStatus(NotificationStatus.Sending, nameof(MarkSent));
        Status = NotificationStatus.Sent;
        SentAt = now;
    }

    /// <summary>
    /// A permanent failure or exhausting all attempts discards the notification; anything else
    /// retries with exponential backoff (2s/4s/8s/16s/32s).
    /// </summary>
    public void MarkFailed(bool permanent, string error, DateTimeOffset now)
    {
        EnsureStatus(NotificationStatus.Sending, nameof(MarkFailed));

        Attempts++;
        LastError = error;

        if (permanent || Attempts >= MaxAttempts)
        {
            Status = NotificationStatus.Discarded;
            return;
        }

        Status = NotificationStatus.Scheduled;
        ScheduledFor = now.Add(Backoff[Math.Min(Attempts - 1, Backoff.Length - 1)]);
    }

    private void EnsureStatus(NotificationStatus expected, string action)
    {
        if (Status != expected)
            throw new InvalidNotificationTransitionException(Id, Status, action);
    }
}
