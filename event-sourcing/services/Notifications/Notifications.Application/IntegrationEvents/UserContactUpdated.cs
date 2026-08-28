namespace Notifications.Application.IntegrationEvents;

public static class IntegrationTopics
{
    public const string UsersContact = "users.contact";
}

/// <summary>
/// This service's own copy of the wire shape published by Users (SPEC.md §5 — "no shared DTO
/// assembly"). Deserialized with System.Text.Json defaults, which ignore unknown members, so an
/// old copy of this class tolerates new fields the producer adds later.
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

public sealed record Envelope<T>(
    Guid EventId,
    string Type,
    Guid AggregateId,
    int Version,
    DateTimeOffset OccurredAt,
    int SchemaVersion,
    T Data);
