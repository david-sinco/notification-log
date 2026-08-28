using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.EventLog;
using Users.Application.ReadModels;
using Users.Domain;
using Users.Infrastructure.EventLog;

namespace Users.Infrastructure.Materializer;

/// <summary>
/// A second, independent consumer of the same users.events topic — deliberately separate from
/// UserMaterializer rather than folded into it, mirroring event-sourcing/SPEC.md §4's
/// inline-vs-async two-projection contrast between UserProfile and VerificationFunnel. Because
/// this only reads (it never produces), it has no command-path race to guard against and needs no
/// shared lock with anything else — just its own fresh-per-boot consumer group, exactly like
/// UserMaterializer's, so a restart always rebuilds from zero. Because the topic is partitioned,
/// this consumer group could in principle scale out one instance per partition with a final merge
/// step — unlike Marten's async daemon, which walks the whole event table single-threaded
/// regardless of how many partitions the equivalent Kafka topic would have (see
/// event-driven/README.md's experiments).
/// </summary>
public sealed class VerificationFunnelMaterializer(IEventLog eventLog, ILogger<VerificationFunnelMaterializer> logger)
    : BackgroundService
{
    private readonly object _gate = new();
    private readonly Dictionary<string, VerificationFunnelBucket> _buckets = [];
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsReady => _ready.Task.IsCompletedSuccessfully;

    public Task WaitUntilReadyAsync(CancellationToken ct) => _ready.Task.WaitAsync(ct);

    public IReadOnlyList<VerificationFunnelBucket> GetAll()
    {
        lock (_gate)
            return _buckets.Values.OrderBy(b => b.Id).ToArray();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Idempotent and safe to call from more than one materializer/process (see IEventLog's
        // own doc comment) — this can't rely on UserMaterializer's EnsureTopicAsync call having
        // already run first: hosted services start concurrently, not in a guaranteed order, and
        // without this a subscribe racing ahead of any topic-creation call risks a broker with
        // auto-create-topics enabled silently creating "users.events" with default settings
        // (1 partition, no retention override) the moment metadata is first requested for it.
        await eventLog.EnsureTopicAsync(UsersEventCodec.Topic, UsersEventCodec.Partitions, UsersEventCodec.TopicConfig, stoppingToken);

        var startedAt = DateTimeOffset.UtcNow;
        var replayed = 0;

        await eventLog.SubscribeAsync(
            UsersEventCodec.Topic,
            consumerGroupId: $"users-funnel-{Guid.NewGuid()}",
            onRecord: (record, _) =>
            {
                var domainEvent = UsersEventCodec.Decode(record.Value);
                lock (_gate)
                    ApplyToBucket(domainEvent);
                replayed++;
                return Task.CompletedTask;
            },
            onCaughtUp: _ =>
            {
                logger.LogInformation(
                    "Verification funnel materializer replayed {Count} events in {Ms}ms, caught up to the current high-watermark.",
                    replayed, (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds);
                _ready.TrySetResult();
                return Task.CompletedTask;
            },
            stoppingToken);
    }

    /// <summary>Caller must already hold _gate. Same event-to-day-bucket routing as
    /// event-sourcing/'s VerificationFunnelProjection: every event lands in the bucket for its
    /// OWN OccurredAt day, not the user's registration day.</summary>
    private void ApplyToBucket(object domainEvent)
    {
        var occurredAt = domainEvent switch
        {
            UserRegistered e => e.OccurredAt,
            EmailChangeRequested e => e.OccurredAt,
            EmailVerified e => e.OccurredAt,
            EmailVerificationFailed e => e.OccurredAt,
            PhoneChangeRequested e => e.OccurredAt,
            PhoneVerified e => e.OccurredAt,
            PhoneVerificationFailed e => e.OccurredAt,
            UserDeactivated e => e.OccurredAt,
            _ => (DateTimeOffset?)null, // PreferencesChanged / UserReactivated: not tracked by the funnel
        };

        if (occurredAt is not { } at)
            return;

        var dayKey = VerificationFunnelBucket.DayKey(at);
        if (!_buckets.TryGetValue(dayKey, out var bucket))
            _buckets[dayKey] = bucket = new VerificationFunnelBucket();

        switch (domainEvent)
        {
            case UserRegistered e: bucket.Apply(e); break;
            case EmailChangeRequested e: bucket.Apply(e); break;
            case EmailVerified e: bucket.Apply(e); break;
            case EmailVerificationFailed e: bucket.Apply(e); break;
            case PhoneChangeRequested e: bucket.Apply(e); break;
            case PhoneVerified e: bucket.Apply(e); break;
            case PhoneVerificationFailed e: bucket.Apply(e); break;
            case UserDeactivated e: bucket.Apply(e); break;
        }
    }
}
