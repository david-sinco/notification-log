using System.Text.Json;
using Users.Domain;

namespace Users.Infrastructure.EventLog;

/// <summary>
/// The wire format for users.events: an explicit envelope with a type discriminator, because two
/// independent consumers (this service's own materializers, and Notifications' contact
/// materializer, each with their own local record types per the "no shared DTO" principle — see
/// Shared/Contracts/schemas/README.md) need to dispatch on event type from raw bytes, and
/// System.Text.Json has no polymorphic deserialization built in for plain records.
/// </summary>
internal static class UsersEventCodec
{
    public const string Topic = "users.events";
    public const int Partitions = 6;
    private const int SchemaVersion = 1;

    /// <summary>
    /// Not compacted, unlike event-sourcing/'s users.contact topic: that topic carries full
    /// snapshots, so keeping only the latest per key is correct. This topic carries raw events
    /// that must ALL survive to be fold-replayed into state — compaction would silently destroy
    /// the ability to reconstruct anything. retention.ms=-1 (infinite) is explicit rather than
    /// "long": a topic that quietly ages out old segments under a default retention would
    /// silently break the full-replay guarantee this whole architecture depends on.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> TopicConfig = new Dictionary<string, string>
    {
        ["cleanup.policy"] = "delete",
        ["retention.ms"] = "-1",
    };

    private sealed record Envelope(
        Guid EventId, string Type, Guid AggregateId, DateTimeOffset OccurredAt, int SchemaVersion, JsonElement Data);

    public static ReadOnlyMemory<byte> Encode(Guid userId, object domainEvent)
    {
        var envelope = new Envelope(
            Guid.NewGuid(), TypeNameOf(domainEvent), userId, OccurredAtOf(domainEvent), SchemaVersion,
            JsonSerializer.SerializeToElement(domainEvent, domainEvent.GetType()));

        return JsonSerializer.SerializeToUtf8Bytes(envelope);
    }

    public static object Decode(ReadOnlyMemory<byte> value)
    {
        var envelope = JsonSerializer.Deserialize<Envelope>(value.Span)
            ?? throw new InvalidOperationException("Empty users.events record.");

        return envelope.Type switch
        {
            nameof(UserRegistered) => envelope.Data.Deserialize<UserRegistered>()!,
            nameof(EmailChangeRequested) => envelope.Data.Deserialize<EmailChangeRequested>()!,
            nameof(EmailVerified) => envelope.Data.Deserialize<EmailVerified>()!,
            nameof(EmailVerificationFailed) => envelope.Data.Deserialize<EmailVerificationFailed>()!,
            nameof(PhoneChangeRequested) => envelope.Data.Deserialize<PhoneChangeRequested>()!,
            nameof(PhoneVerified) => envelope.Data.Deserialize<PhoneVerified>()!,
            nameof(PhoneVerificationFailed) => envelope.Data.Deserialize<PhoneVerificationFailed>()!,
            nameof(PreferencesChanged) => envelope.Data.Deserialize<PreferencesChanged>()!,
            nameof(UserDeactivated) => envelope.Data.Deserialize<UserDeactivated>()!,
            nameof(UserReactivated) => envelope.Data.Deserialize<UserReactivated>()!,
            var unknown => throw new InvalidOperationException($"Unknown users.events type '{unknown}'."),
        };
    }

    private static string TypeNameOf(object e) => e switch
    {
        UserRegistered => nameof(UserRegistered),
        EmailChangeRequested => nameof(EmailChangeRequested),
        EmailVerified => nameof(EmailVerified),
        EmailVerificationFailed => nameof(EmailVerificationFailed),
        PhoneChangeRequested => nameof(PhoneChangeRequested),
        PhoneVerified => nameof(PhoneVerified),
        PhoneVerificationFailed => nameof(PhoneVerificationFailed),
        PreferencesChanged => nameof(PreferencesChanged),
        UserDeactivated => nameof(UserDeactivated),
        UserReactivated => nameof(UserReactivated),
        _ => throw new ArgumentOutOfRangeException(nameof(e), e.GetType(), "Unknown Users event type."),
    };

    private static DateTimeOffset OccurredAtOf(object e) => e switch
    {
        UserRegistered x => x.OccurredAt,
        EmailChangeRequested x => x.OccurredAt,
        EmailVerified x => x.OccurredAt,
        EmailVerificationFailed x => x.OccurredAt,
        PhoneChangeRequested x => x.OccurredAt,
        PhoneVerified x => x.OccurredAt,
        PhoneVerificationFailed x => x.OccurredAt,
        PreferencesChanged x => x.OccurredAt,
        UserDeactivated x => x.OccurredAt,
        UserReactivated x => x.OccurredAt,
        _ => throw new ArgumentOutOfRangeException(nameof(e), e.GetType(), "Unknown Users event type."),
    };
}
