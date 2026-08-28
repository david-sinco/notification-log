namespace Shared.EventLog;

/// <summary>One record read back off the log, with the position a consumer needs to track.</summary>
public sealed record LogRecord(int Partition, long Offset, string Key, ReadOnlyMemory<byte> Value);

/// <summary>
/// The log-as-database abstraction (event-driven/README.md). Two structural differences from
/// event-sourcing/'s <c>Shared/Messaging.IMessageTransport</c> drive every method here:
///
/// 1. <see cref="ProduceAsync"/> returns the partition+offset the record landed on. A command
///    handler needs this to advance its own "applied through offset" watermark the instant it
///    self-applies the event it just wrote (see Users.Infrastructure's materializer) — with a
///    plain fire-and-forget publish there'd be no way to know.
/// 2. <see cref="SubscribeAsync"/> takes an <c>onCaughtUp</c> callback, fired once the consumer
///    has read through to the topic's high-watermark at subscribe time. Every consumer in this
///    solution rebuilds in-memory state by replaying from the beginning on every process boot
///    (nothing durable survives a restart except Kafka itself) and must not serve traffic until
///    that replay finishes — this is how a caller finds out "done replaying, safe to answer
///    requests now" without polling.
/// </summary>
public interface IEventLog
{
    /// <summary>
    /// Ensures <paramref name="topic"/> exists with the given partition count and configs
    /// (idempotent — a second caller racing to create the same topic is not an error). Configs are
    /// caller-supplied rather than hardcoded, unlike event-sourcing/'s KafkaTransport which always
    /// creates compacted topics: this solution's one topic is deliberately NOT compacted (raw
    /// events must all survive to be fold-replayed), so hardcoding compaction here would be wrong.
    /// </summary>
    Task EnsureTopicAsync(
        string topic, int partitions, IReadOnlyDictionary<string, string> configs, CancellationToken ct);

    Task<(int Partition, long Offset)> ProduceAsync(
        string topic, string key, ReadOnlyMemory<byte> value, CancellationToken ct);

    /// <summary>
    /// Reads every partition of <paramref name="topic"/> from the true beginning (via explicit
    /// partition assignment, not consumer-group offset commits — see KafkaEventLog for why) and
    /// keeps delivering records to <paramref name="onRecord"/> until <paramref name="ct"/> is
    /// cancelled. <paramref name="consumerGroupId"/> is a label for the Kafka UI, not a resume
    /// point: every call replays from offset 0 regardless of what a prior process instance
    /// committed. <paramref name="onCaughtUp"/>, if given, fires exactly once, after the initial
    /// replay reaches each partition's high-watermark as observed at subscribe time.
    /// </summary>
    Task SubscribeAsync(
        string topic,
        string consumerGroupId,
        Func<LogRecord, CancellationToken, Task> onRecord,
        Func<CancellationToken, Task>? onCaughtUp,
        CancellationToken ct);
}
