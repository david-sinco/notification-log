using System.Collections.Concurrent;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Shared.EventLog;

/// <summary>
/// Confluent.Kafka-backed <see cref="IEventLog"/>. See the type's own doc comment for why this
/// isn't event-sourcing/'s KafkaTransport reused: offsets need to come back out of a produce, and
/// subscribers need an explicit "caught up" signal.
/// </summary>
public sealed class KafkaEventLog(string bootstrapServers) : IEventLog, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, byte> _ensuredTopics = new();
    private readonly Lazy<IProducer<string, byte[]>> _producer = new(() => new ProducerBuilder<string, byte[]>(
        new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All,
            LingerMs = 5,
        }).Build());

    private readonly List<IConsumer<string, byte[]>> _consumers = [];

    public async Task EnsureTopicAsync(
        string topic, int partitions, IReadOnlyDictionary<string, string> configs, CancellationToken ct)
    {
        if (_ensuredTopics.ContainsKey(topic))
            return;

        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
        try
        {
            await admin.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = partitions,
                    ReplicationFactor = 1,
                    Configs = new Dictionary<string, string>(configs),
                },
            ], new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(10) });
        }
        catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            // Fine — another instance (or a prior boot of this same process) created it first.
        }

        _ensuredTopics.TryAdd(topic, 0);
    }

    public async Task<(int Partition, long Offset)> ProduceAsync(
        string topic, string key, ReadOnlyMemory<byte> value, CancellationToken ct)
    {
        var result = await _producer.Value.ProduceAsync(
            topic, new Message<string, byte[]> { Key = key, Value = value.ToArray() }, ct);

        return (result.Partition.Value, result.Offset.Value);
    }

    public async Task SubscribeAsync(
        string topic,
        string consumerGroupId,
        Func<LogRecord, CancellationToken, Task> onRecord,
        Func<CancellationToken, Task>? onCaughtUp,
        CancellationToken ct)
    {
        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();
        var metadata = admin.GetMetadata(topic, TimeSpan.FromSeconds(10));
        var partitionIds = metadata.Topics.Single(t => t.Topic == topic).Partitions.Select(p => p.PartitionId).ToArray();

        var consumer = new ConsumerBuilder<string, byte[]>(new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = consumerGroupId,
            EnableAutoCommit = false,
        }).Build();

        lock (_consumers)
            _consumers.Add(consumer);

        // Assign, not Subscribe: this solution's in-memory state is wiped on every restart, so a
        // consumer must always replay from the true beginning regardless of what a prior process
        // instance committed under this same group id. Assign + Offset.Beginning sidesteps
        // consumer-group offset commits entirely rather than relying on a fresh-per-boot group id
        // to avoid resuming a stale cursor — consumerGroupId ends up being a Kafka-UI label only.
        var assignment = partitionIds.Select(p => new TopicPartitionOffset(topic, p, Offset.Beginning)).ToList();
        consumer.Assign(assignment);

        // High-watermark at subscribe time, captured up front, is the target "caught up" has to
        // reach — not the ever-moving live watermark, or a busy topic would never fire onCaughtUp.
        var remaining = new HashSet<int>();
        foreach (var partitionId in partitionIds)
        {
            var watermarks = consumer.QueryWatermarkOffsets(new TopicPartition(topic, partitionId), TimeSpan.FromSeconds(10));
            if (watermarks.High.Value > watermarks.Low.Value)
                remaining.Add(partitionId);
        }

        var caughtUpSignaled = remaining.Count == 0;
        if (caughtUpSignaled && onCaughtUp is not null)
            await onCaughtUp(ct);

        var highWatermarks = partitionIds.ToDictionary(
            p => p, p => consumer.QueryWatermarkOffsets(new TopicPartition(topic, p), TimeSpan.FromSeconds(10)).High.Value);

        _ = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                ConsumeResult<string, byte[]>? result;
                try
                {
                    result = consumer.Consume(ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException)
                {
                    continue;
                }

                if (result is null || result.IsPartitionEOF)
                    continue;

                try
                {
                    await onRecord(
                        new LogRecord(result.Partition.Value, result.Offset.Value, result.Message.Key, result.Message.Value ?? []),
                        ct);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    // An uncaught exception here would otherwise silently kill this entire
                    // Task.Run loop — no more records ever delivered to this subscriber again,
                    // with nothing in the logs to explain why. There's no commit-based redelivery
                    // to fall back on (this consumer uses explicit Assign + Beginning, not
                    // group-coordinated offsets — see the comment above), so this record is lost
                    // to this subscriber; logging loudly and moving on beats the alternative.
                    Console.Error.WriteLine(
                        $"[KafkaEventLog] onRecord failed for {topic} partition {result.Partition.Value} offset {result.Offset.Value}: {ex}");
                }

                if (!caughtUpSignaled && result.Offset.Value >= highWatermarks[result.Partition.Value] - 1)
                {
                    remaining.Remove(result.Partition.Value);
                    if (remaining.Count == 0)
                    {
                        caughtUpSignaled = true;
                        if (onCaughtUp is not null)
                            await onCaughtUp(ct);
                    }
                }
            }
        }, ct);
    }

    public ValueTask DisposeAsync()
    {
        if (_producer.IsValueCreated)
            _producer.Value.Dispose();

        lock (_consumers)
        {
            foreach (var consumer in _consumers)
            {
                consumer.Close();
                consumer.Dispose();
            }
        }

        return ValueTask.CompletedTask;
    }
}
