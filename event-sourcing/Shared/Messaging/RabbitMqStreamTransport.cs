using System.Buffers;
using System.Collections.Concurrent;
using System.Net;
using RabbitMQ.Stream.Client;
using RabbitMQ.Stream.Client.AMQP;
using RabbitMQ.Stream.Client.Reliable;

namespace Shared.Messaging;

/// <summary>
/// RabbitMQ Streams — a different sub-protocol of the same broker from RabbitMqTransport, not a
/// configuration flag on it. Where a classic queue deletes a message on ack, a stream is a real
/// append-only log: every publish is retained (up to MaxLengthBytes), any consumer can start
/// from any offset, and re-reading from the beginning always reproduces the same sequence
/// (SPEC.md §7's "evaluate the log" axis).
///
/// One structural property worth noting against Kafka: a stream here is never partitioned (no
/// Super Stream in this lab), so ordering within one stream is a *total* order across every key,
/// not merely a per-partition order the way Kafka's is. Kafka trades that away for horizontal
/// consumer parallelism; this transport doesn't need it at this lab's volume.
///
/// "Consumer group" maps onto <c>ConsumerConfig.Reference</c>: multiple references can read the
/// same stream independently, each remembering its own last-processed offset via
/// StreamSystem.StoreOffset/TryQueryOffset — the direct analogue of a Kafka committed offset,
/// something classic RabbitMQ queues have no equivalent of at all.
/// </summary>
public sealed class RabbitMqStreamTransport : IMessageTransport, IAsyncDisposable
{
    private const int DefaultMaxLengthBytes = 500_000_000;

    private readonly string _host;
    private readonly int _port;
    private readonly string _username;
    private readonly string _password;
    private readonly string _virtualHost;

    private readonly SemaphoreSlim _systemLock = new(1, 1);
    private readonly SemaphoreSlim _producersLock = new(1, 1);
    private readonly ConcurrentDictionary<string, byte> _declaredStreams = new();
    private readonly ConcurrentDictionary<string, Producer> _producers = new();
    private readonly List<Consumer> _consumers = [];
    private StreamSystem? _system;

    /// <summary>
    /// Reuses the same broker credentials as classic RabbitMQ (parsed out of the AMQP connection
    /// string Aspire already hands every service) but talks to it over the Streams protocol port,
    /// which AppHost exposes separately since it's a different listener on the same container.
    /// </summary>
    public RabbitMqStreamTransport(string amqpConnectionString, int streamPort)
    {
        var uri = new Uri(amqpConnectionString);
        _host = uri.Host;
        _port = streamPort;

        var userInfo = uri.UserInfo.Split(':', 2);
        _username = userInfo.Length > 0 && userInfo[0].Length > 0 ? Uri.UnescapeDataString(userInfo[0]) : "guest";
        _password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "guest";

        var vhostPath = uri.AbsolutePath.TrimStart('/');
        _virtualHost = string.IsNullOrEmpty(vhostPath) ? "/" : Uri.UnescapeDataString(vhostPath);
    }

    public async Task PublishAsync(string topic, string partitionKey, ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        var system = await GetSystemAsync(ct);
        await EnsureStreamAsync(system, topic, ct);
        var producer = await GetProducerAsync(system, topic, ct);

        var message = new Message(body.ToArray())
        {
            ApplicationProperties = new ApplicationProperties(),
        };
        message.ApplicationProperties["partitionKey"] = partitionKey;

        await producer.Send(message);
    }

    public async Task SubscribeAsync(string topic, string consumerGroup,
        Func<ReadOnlyMemory<byte>, CancellationToken, Task> handler, CancellationToken ct)
    {
        var system = await GetSystemAsync(ct);
        await EnsureStreamAsync(system, topic, ct);

        // No stored offset for this reference yet -> OffsetTypeFirst replays the whole retained
        // log from the start, exactly Kafka's Earliest. This is what makes "bootstrap a brand new
        // consumer from empty" possible on RabbitMQ at all (SPEC.md §11 experiment 3) — classic
        // queues structurally cannot do this, see RabbitMqTransport's header comment.
        var lastStoredOffset = await system.TryQueryOffset(consumerGroup, topic);
        IOffsetType offsetSpec = lastStoredOffset is { } offset
            ? new OffsetTypeOffset(offset + 1)
            : new OffsetTypeFirst();

        var consumerConfig = new ConsumerConfig(system, topic)
        {
            Reference = consumerGroup,
            OffsetSpec = offsetSpec,
            MessageHandler = async (_, rawConsumer, context, message) =>
            {
                try
                {
                    var payload = message.Data.Contents.ToArray();
                    await handler(payload, ct);

                    // Persisted server-side under (Reference, stream) so the next SubscribeAsync
                    // call for this same consumerGroup resumes right after here, not from First
                    // again — the checkpoint TryQueryOffset reads above.
                    await rawConsumer.StoreOffset(context.Offset);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"[RabbitMqStreamTransport] handler failed for {topic}/{consumerGroup} at offset {context.Offset}: {ex}");

                    // Deliberately do not store the offset: unlike RabbitMqTransport's nack (which
                    // discards just the one poison message), a stream has no per-message ack —
                    // leaving the checkpoint behind means this offset is redelivered, and every
                    // offset after it, on the next connect. Closer to Kafka's manual-commit
                    // behaviour than to classic RabbitMQ's.
                }
            },
        };

        var consumer = await Consumer.Create(consumerConfig);
        lock (_consumers)
            _consumers.Add(consumer);
    }

    private async Task<Producer> GetProducerAsync(StreamSystem system, string topic, CancellationToken ct)
    {
        if (_producers.TryGetValue(topic, out var existing))
            return existing;

        await _producersLock.WaitAsync(ct);
        try
        {
            if (_producers.TryGetValue(topic, out existing))
                return existing;

            var producer = await Producer.Create(new ProducerConfig(system, topic));
            _producers[topic] = producer;
            return producer;
        }
        finally
        {
            _producersLock.Release();
        }
    }

    private async Task EnsureStreamAsync(StreamSystem system, string topic, CancellationToken ct)
    {
        if (_declaredStreams.ContainsKey(topic))
            return;

        if (!await system.StreamExists(topic))
        {
            await system.CreateStream(new StreamSpec(topic) { MaxLengthBytes = DefaultMaxLengthBytes });
        }

        _declaredStreams.TryAdd(topic, 0);
    }

    private async Task<StreamSystem> GetSystemAsync(CancellationToken ct)
    {
        if (_system is { IsClosed: false })
            return _system;

        await _systemLock.WaitAsync(ct);
        try
        {
            if (_system is { IsClosed: false })
                return _system;

            _system = await StreamSystem.Create(new StreamSystemConfig
            {
                UserName = _username,
                Password = _password,
                VirtualHost = _virtualHost,
                Endpoints = [new DnsEndPoint(_host, _port)],
            });
            return _system;
        }
        finally
        {
            _systemLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var consumer in _consumers)
            await consumer.Close();

        foreach (var producer in _producers.Values)
            await producer.Close();

        if (_system is not null)
            await _system.DisposeAsync();

        _systemLock.Dispose();
        _producersLock.Dispose();
    }
}
