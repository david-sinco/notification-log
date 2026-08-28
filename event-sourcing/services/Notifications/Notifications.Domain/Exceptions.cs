namespace Notifications.Domain;

public class NotificationDomainException : Exception
{
    public NotificationDomainException(string message) : base(message) { }
}

/// <summary>Raised when the replica has no record of this user yet. Maps to HTTP 404.</summary>
public sealed class UnknownUserException : NotificationDomainException
{
    public UnknownUserException(Guid userId)
        : base($"No replicated contact for user '{userId}' yet.") { }
}

/// <summary>Raised when scheduling a channel the user can't currently receive on. Maps to HTTP 422.</summary>
public sealed class ChannelNotReceivableException : NotificationDomainException
{
    public ChannelNotReceivableException(Guid userId, NotificationChannel channel)
        : base($"User '{userId}' cannot receive on channel '{channel}' right now.") { }
}

/// <summary>Raised by an illegal state-machine transition. Maps to HTTP 422.</summary>
public sealed class InvalidNotificationTransitionException : NotificationDomainException
{
    public InvalidNotificationTransitionException(Guid notificationId, NotificationStatus actual, string action)
        : base($"Cannot {action} notification '{notificationId}' while it is '{actual}'.") { }
}

/// <summary>
/// Raised when the idempotency-key unique index rejects a duplicate schedule request. Maps to
/// HTTP 409.
/// </summary>
public sealed class DuplicateNotificationException : NotificationDomainException
{
    public DuplicateNotificationException(string idempotencyKey)
        : base($"A notification with idempotency key '{idempotencyKey}' already exists.") { }
}
