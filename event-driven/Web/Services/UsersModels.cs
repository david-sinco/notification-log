using System.Text.Json;

namespace Web.Services;

/// <summary>
/// This console's own copies of Users.Api's request/response shapes — it talks HTTP+JSON like
/// curl would, not a project reference, so it stays a true outside-in client of the API surface.
///
/// Unlike event-sourcing/'s Web, there is no separate UserProfileDto: GET /api/users/{id} returns
/// the same shape as every other Users.Api endpoint, because there's only one place state lives
/// in this architecture (the in-memory fold) — see event-driven/README.md.
/// </summary>
public sealed record PreferencesDto(bool Email, bool Sms, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd);

public sealed record UserDto(
    Guid Id, string Name, string? VerifiedEmail, string? VerifiedPhone,
    bool HasPendingEmail, bool HasPendingPhone, bool IsActive, int Version, PreferencesDto Preferences);

public sealed record VerificationTokenDto(UserDto User, string Token);

/// <summary>
/// GET /api/users/{id}/events returns { type, data } per entry — no top-level version or
/// occurredAt the way event-sourcing/'s Marten-backed stream exposes them, because this is a
/// plain in-memory list, not a queried event store; OccurredAt still exists, just nested inside
/// Data like every other domain-event field.
/// </summary>
public sealed record EventEntryDto(string Type, JsonElement Data);

public sealed record FunnelBucketDto(
    string Id, int Registrations, int EmailChangesRequested, int EmailsVerified,
    int PhoneChangesRequested, int PhonesVerified, int Deactivations,
    int TokenMismatchFailures, int TokenExpiredFailures, int TooManyAttemptsFailures, double ConversionRate);
