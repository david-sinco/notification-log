using System.Text.Json;

namespace Web.Services;

/// <summary>
/// This console's own copies of Users.Api's request/response shapes — it talks HTTP+JSON like
/// curl would, not a project reference, so it stays a true outside-in client of the API surface.
/// </summary>
public sealed record PreferencesDto(bool Email, bool Sms, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd);

public sealed record UserDto(
    Guid Id, string Name, string? VerifiedEmail, string? VerifiedPhone,
    bool HasPendingEmail, bool HasPendingPhone, bool IsActive, int Version, PreferencesDto Preferences);

public sealed record VerificationTokenDto(UserDto User, string Token);

// UserProfile is returned as-is by GET /api/users/{id} (no response-DTO mapping), so its
// Preferences shape is Domain.NotificationPreferences's own field names — EmailEnabled/
// SmsEnabled — not the Email/Sms shape every other Users.Api endpoint uses via PreferencesResponse.
public sealed record ProfilePreferencesDto(bool EmailEnabled, bool SmsEnabled, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd);

public sealed record UserProfileDto(
    Guid Id, string Name, string? Email, bool EmailVerified, string? Phone, bool PhoneVerified,
    bool Active, ProfilePreferencesDto Preferences, int Version, DateTimeOffset UpdatedAt);

public sealed record EventEntryDto(string Type, long Version, DateTimeOffset OccurredAt, JsonElement Data);

public sealed record FunnelBucketDto(
    string Id, int Registrations, int EmailChangesRequested, int EmailsVerified,
    int PhoneChangesRequested, int PhonesVerified, int Deactivations,
    int TokenMismatchFailures, int TokenExpiredFailures, int TooManyAttemptsFailures, double ConversionRate);
