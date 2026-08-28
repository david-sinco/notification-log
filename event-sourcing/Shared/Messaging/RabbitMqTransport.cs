using System.Collections.Concurrent;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Shared.Messaging;

/// <summary>
/// Topic exchange per topic, durable queue per (topic, consumer group), manual ack, bounded
/// prefetch, nack-without-requeue on handler failure so a poison message can't spin forever
/// (SPEC.md §7).
///
/// What this structurally cannot do, unlike Kafka:
///   - A queue only receives messages published *after* it was declared and bound. There is no
///     history: a new consumer group starts from zero regardless of what already happened.
///   - Acking a message deletes it. A bug in the handler that acked before finishing is
///     unrecoverable — the message is gone, not "replayable."
///   - A new consumer group needs its own bootstrap path (e.g. an API to pull current state);
///     RabbitMQ itself has no equivalent of resetting a Kafka consumer group's offset to replay.
/// </summary>
public sealed class RabbitMqTransport : IMessageTransport, IAsyncDisposable
{
    private const ushort PrefetchCount = 32;

    private readonly ConnectionFactory _factory;
    private readonly SemaphoreSlim _connectLock = new(1, 1);
    private readonly ConcurrentDictionary<string, byte> _declaredExchanges = new();
    private IConnection? _connection;
    private IChannel? _publishChannel;

    public RabbitMqTransport(string connectionString)
    {
        _factory = new ConnectionFactory { Uri = new Uri(connectionString) };
    }

    public async Task PublishAsync(string topic, string partitionKey, ReadOnlyMemory<byte> body, CancellationToken ct)
    {
        var channel = await GetPublishChannelAsync(ct);
        await EnsureExchangeDeclaredAsync(channel, topic, ct);

        var props = new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            Headers = new Dictionary<string, object?> { ["partitionKey"] = partitionKey },
        };

        await channel.BasicPublishAsync(topic, routingKey: topic, mandatory: false, props, body, ct);
    }

    public async Task SubscribeAsync(string topic, string consumerGroup,
        Func<ReadOnlyMemory<byte>, CancellationToken, Task> handler, CancellationToken ct)
    {
        var connection = await GetConnectionAsync(ct);
        var channel = await connection.CreateChannelAsync(cancellationToken: ct);
        await EnsureExchangeDeclaredAsync(channel, topic, ct);

        var queueName = $"{consumerGroup}.{topic}";
        await channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct);
        await channel.QueueBindAsync(queueName, topic, routingKey: topic, cancellationToken: ct);
        await channel.BasicQosAsync(0, PrefetchCount, global: false, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, delivery) =>
        {
            try
            {
                // Copy immediately: delivery.Body is backed by a buffer the client can recycle
                // for the next frame once this handler yields, so holding onto it across an
                // await is unsafe.
                var body = delivery.Body.ToArray();
                await handler(body, ct);
                await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, ct);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[RabbitMqTransport] handler failed for {topic}, deliveryTag={delivery.DeliveryTag}: {ex}");
                // requeue: false — a handler that keeps failing must not spin the same poison
                // message forever. It's lost, which is exactly the "ack deletes it" trade-off
                // documented on this type.
                await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false, requeue: false, ct);
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer, cancellationToken: ct);
    }

    private async Task EnsureExchangeDeclaredAsync(IChannel channel, string topic, CancellationToken ct)
    {
        if (_declaredExchanges.ContainsKey(topic))
            return;

        await channel.ExchangeDeclareAsync(topic, ExchangeType.Topic, durable: true, cancellationToken: ct);
        _declaredExchanges.TryAdd(topic, 0);
    }

    private async Task<IChannel> GetPublishChannelAsync(CancellationToken ct)
    {
        if (_publishChannel is { IsOpen: true })
            return _publishChannel;

        var connection = await GetConnectionAsync(ct);
        _publishChannel = await connection.CreateChannelAsync(cancellationToken: ct);
        return _publishChannel;
    }

    private async Task<IConnection> GetConnectionAsync(CancellationToken ct)
    {
        if (_connection is { IsOpen: true })
            return _connection;

        await _connectLock.WaitAsync(ct);
        try
        {
            if (_connection is { IsOpen: true })
                return _connection;

            _connection = await _factory.CreateConnectionAsync(ct);
            return _connection;
        }
        finally
        {
            _connectLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_publishChannel is not null)
            await _publishChannel.DisposeAsync();

        if (_connection is not null)
            await _connection.DisposeAsync();

        _connectLock.Dispose();
    }
}
