using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notifications.Domain;
using Shared.EventLog;

namespace Notifications.Infrastructure.EventLog;

/// <summary>
/// Reconstructs the contact replica by folding users.events DIRECTLY — the deliberate departure
/// from event-sourcing/SPEC.md §5's "domain events never leave the service" rule, and the actual
/// point of this second example: Notifications rebuilds its own state from the same shared
/// canonical log Users writes, with zero coupling to a Users API and zero dependency on Users
/// choosing to translate/publish anything (event-driven/README.md). It defines its own local
/// record shapes (Notifications.Domain.RawUsersEvents.cs) rather than referencing Users.Domain,
/// and recognizes only six of the ten event types on the wire; the other four
/// (EmailChangeRequested, PhoneChangeRequested, EmailVerificationFailed, PhoneVerificationFailed)
/// are irrelevant to a contact replica and are skipped without logging, not treated as a surprise
/// — the direct analogue of event-sourcing/'s UsersContactConsumer logging a warning (not
/// crashing) on a schemaVersion higher than it understands, now applied to an unfamiliar type tag
/// on a shared multi-type topic.
///
/// A single sequential consumer, so — unlike Users.Infrastructure.Materializer.UserMaterializer —
/// there is no self-produced-write race to guard against and no offset watermark needed: this
/// service only ever reads.
/// </summary>
public sealed class ContactMaterializer(IEventLog eventLog, ILogger<ContactMaterializer> logger) : BackgroundService
{
    // Duplicated from Users.Infrastructure.EventLog.UsersEventCodec — the two services share no
    // project for these, the same "no shared DTO" duplication as the record shapes above; both
    // sides simply agree on the topic's name and config by convention, like any other consumer
    // coupling to a producer's wire contract. The config matters here, not just the name: this is
    // a different process than Users.Api, so it can't rely on Users.Api having created the topic
    // first — see EnsureTopicAsync below.
    private const string Topic = "users.events";
    private const int Partitions = 6;

    private static readonly IReadOnlyDictionary<string, string> TopicConfig = new Dictionary<string, string>
    {
        ["cleanup.policy"] = "delete",
        ["retention.ms"] = "-1",
    };

    private readonly object _gate = new();
    private readonly Dictionary<Guid, UserContact> _contacts = [];
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsReady => _ready.Task.IsCompletedSuccessfully;

    public Task WaitUntilReadyAsync(CancellationToken ct) => _ready.Task.WaitAsync(ct);

    public UserContact? TryGet(Guid userId)
    {
        lock (_gate)
            return _contacts.GetValueOrDefault(userId);
    }

    public int Count()
    {
        lock (_gate)
            return _contacts.Count;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Idempotent (see IEventLog's own doc comment) and necessary here specifically: this is a
        // different process than Users.Api, with no guaranteed startup order relative to it, so a
        // subscribe racing ahead of any topic-creation call risks a broker with auto-create-topics
        // enabled silently creating "users.events" with default settings (1 partition, no
        // retention override) the moment metadata is first requested for it.
        await eventLog.EnsureTopicAsync(Topic, Partitions, TopicConfig, stoppingToken);

        var startedAt = DateTimeOffset.UtcNow;
        var replayed = 0;

        await eventLog.SubscribeAsync(
            Topic,
            consumerGroupId: $"notifications-contact-{Guid.NewGuid()}",
            onRecord: (record, _) =>
            {
                Apply(record.Value);
                replayed++;
                return Task.CompletedTask;
            },
            onCaughtUp: _ =>
            {
                logger.LogInformation(
                    "Contact materializer replayed {Count} events in {Ms}ms, caught up to the current high-watermark.",
                    replayed, (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
                _ready.TrySetResult();
                return Task.CompletedTask;
            },
            stoppingToken);
    }

    private void Apply(ReadOnlyMemory<byte> rawValue)
    {
        var envelope = JsonSerializer.Deserialize<Envelope>(rawValue.Span);
        if (envelope is null)
            return;

        lock (_gate)
        {
            switch (envelope.Type)
            {
                case "UserRegistered":
                    var registered = envelope.Data.Deserialize<UserRegisteredEvent>()!;
                    _contacts[registered.UserId] = UserContact.FromRegistration(registered);
                    break;

                case "EmailVerified":
                    ApplyExisting(envelope, c => c.Apply(envelope.Data.Deserialize<EmailVerifiedEvent>()!));
                    break;

                case "PhoneVerified":
                    ApplyExisting(envelope, c => c.Apply(envelope.Data.Deserialize<PhoneVerifiedEvent>()!));
                    break;

                case "PreferencesChanged":
                    ApplyExisting(envelope, c => c.Apply(envelope.Data.Deserialize<PreferencesChangedEvent>()!));
                    break;

                case "UserDeactivated":
                    ApplyExisting(envelope, c => c.Apply(envelope.Data.Deserialize<UserDeactivatedEvent>()!));
                    break;

                case "UserReactivated":
                    ApplyExisting(envelope, c => c.Apply(envelope.Data.Deserialize<UserReactivatedEvent>()!));
                    break;

                case "EmailChangeRequested" or "PhoneChangeRequested" or "EmailVerificationFailed" or "PhoneVerificationFailed":
                    break; // expected-and-irrelevant to a contact replica — not a surprise, nothing to log

                default:
                    logger.LogWarning("Received unrecognized users.events type '{Type}'; skipping.", envelope.Type);
                    break;
            }
        }
    }

    /// <summary>Caller must already hold _gate.</summary>
    private void ApplyExisting(Envelope envelope, Action<UserContact> apply)
    {
        if (!_contacts.TryGetValue(envelope.AggregateId, out var contact))
        {
            logger.LogWarning(
                "Received {Type} for unknown user {UserId} before UserRegistered; dropping.",
                envelope.Type, envelope.AggregateId);
            return;
        }

        apply(contact);
    }

    private sealed record Envelope(
        Guid EventId, string Type, Guid AggregateId, DateTimeOffset OccurredAt, int SchemaVersion, JsonElement Data);
}
