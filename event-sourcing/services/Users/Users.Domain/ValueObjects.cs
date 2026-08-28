namespace Users.Domain;

public sealed record PendingContact(
    string Value, string TokenHash, DateTimeOffset ExpiresAt, int FailedAttempts);

public sealed record NotificationPreferences(
    bool EmailEnabled, bool SmsEnabled, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd)
{
    public static NotificationPreferences Default { get; } = new(
        EmailEnabled: true, SmsEnabled: false, QuietHoursStart: null, QuietHoursEnd: null);
}

public enum VerificationFailureReason
{
    TokenMismatch,
    TokenExpired,
    TooManyAttempts,
}
