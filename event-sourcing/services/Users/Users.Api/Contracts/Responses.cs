using Users.Domain;

namespace Users.Api.Contracts;

public sealed record PreferencesResponse(bool Email, bool Sms, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd)
{
    public static PreferencesResponse From(NotificationPreferences p) =>
        new(p.EmailEnabled, p.SmsEnabled, p.QuietHoursStart, p.QuietHoursEnd);
}

public sealed record UserResponse(
    Guid Id,
    string Name,
    string? VerifiedEmail,
    string? VerifiedPhone,
    bool HasPendingEmail,
    bool HasPendingPhone,
    bool IsActive,
    int Version,
    PreferencesResponse Preferences)
{
    public static UserResponse From(User user) => new(
        user.Id, user.Name, user.VerifiedEmail, user.VerifiedPhone,
        user.PendingEmail is not null, user.PendingPhone is not null,
        user.IsActive, user.Version, PreferencesResponse.From(user.Preferences));
}

/// <summary>
/// Returning the plaintext token over HTTP is a lab affordance so the verification flow is
/// drivable by curl (SPEC.md §4). A real system would deliver it out-of-band (email/SMS) and
/// never echo it back to the caller that requested the change.
/// </summary>
public sealed record VerificationTokenResponse(UserResponse User, string Token)
{
    public static VerificationTokenResponse From(Application.VerificationTokenResult result) =>
        new(UserResponse.From(result.User), result.Token);
}
