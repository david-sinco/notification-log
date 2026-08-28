using System.Collections.Concurrent;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Shared.Messaging;

/// <summary>
/// Topics are created compacted with a short segment.ms so compaction is observable within a
/// short lab session. Messages are keyed by partitionKey, which both pins ordering (same key,
/// same partition) and is the key compaction dedupes on (SPEC.md §7).
///
/// Consumer commits are manual and only happen after the handler succeeds — on failure the
/// offset is left alone so the message is retried, unlike RabbitMQ where a failed handler loses
/// the message on nack.
/// </summary>
public sealed class KafkaTransport : IMessageTransport, IAsyncDisposable
{
    private readonly string _bootstrapServers;
    private readonly int _partitions;
    private readonly string _segmentMs;
    private readonly ConcurrentDictionary<string, byte> _ensuredTopics = new();
    private readonly Lazy<IProducer<string, byte[]>> _producer;
    private readonly List<IConsumer<string, byte[]>> _consumers = [];

    public KafkaTransport(string bootstrapServers, int partitions = 6, string segmentMs = "60000")
    {
        _bootstrapServers = bootstrapServers;
        _partitions = partitions;
        _segmentMs = segmentMs;
        _producer = new Lazy<IProducer<string, byte[]>>(() => new ProducerBuilder<string, byte[]>(new ProducerConfig
        {
            BootstrapServers = _bootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All,
            LingerMs = 5,
        }).Build());
    }

    public async Task PublishAsync(string topic, string partitionKey, ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        await EnsureTopicAsync(topic, ct);

        // An empty body publishes a tombstone: Kafka's compaction drops everything for this key
        // once it also drops the tombstone itself after the retention window.
        var message = new Message<string, byte[]>
        {
            Key = partitionKey,
            Value = body.IsEmpty ? null! : body.ToArray(),
        };

        await _producer.Value.ProduceAsync(topic, message, ct);
    }

    public async Task SubscribeAsync(string topic, string consumerGroup,
        Func<ReadOnlyMemory<byte>, CancellationToken, Task> handler, CancellationToken ct)
    {
        await EnsureTopicAsync(topic, ct);

        var consumer = new ConsumerBuilder<string, byte[]>(new ConsumerConfig
        {
            BootstrapServers = _bootstrapServers,
            GroupId = consumerGroup,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
        }).Build();

        lock (_consumers)
            _consumers.Add(consumer);

        consumer.Subscribe(topic);

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
                    // A null value is a tombstone (SPEC.md §7) — handlers are expected to treat
                    // an empty body as "this key was deleted."
                    var value = result.Message.Value ?? [];
                    await handler(value, ct);
                    consumer.Commit(result);
                }
                catch
                {
                    // Do not commit: the offset stays put and this message is redelivered on the
                    // next poll, unlike RabbitMQ's nack-and-lose-it.
                }
            }
        }, ct);
    }

    private async Task EnsureTopicAsync(string topic, CancellationToken ct)
    {
        if (_ensuredTopics.ContainsKey(topic))
            return;

        using var admin = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = _bootstrapServers }).Build();
        try
        {
            await admin.CreateTopicsAsync(
            [
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = _partitions,
                    ReplicationFactor = 1,
                    Configs = new Dictionary<string, string>
                    {
                        ["cleanup.policy"] = "compact",
                        ["segment.ms"] = _segmentMs,
                    },
                },
            ], new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(10) });
        }
        catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            // Fine — another instance created it first.
        }

        _ensuredTopics.TryAdd(topic, 0);
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
