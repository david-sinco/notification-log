namespace Shared.Messaging;

/// <summary>
/// The entire transport abstraction, on purpose (SPEC.md §7). Do not widen this interface itself
/// to paper over what a given implementation can't do — RabbitMqTransport (classic queues) still
/// has no consumer-group replay and KafkaTransport still compacts by physical offset, not by any
/// field in the payload; RabbitMqStreamTransport sits between them, giving replay-from-offset and
/// durable per-reference checkpoints without partitions or compaction. The differences are the
/// point (SPEC.md §11); hiding them behind a fatter interface would defeat the lab. Selected via
/// config (Messaging:Kind = RabbitMq | RabbitMqStream | Kafka).
/// </summary>
public interface IMessageTransport
{
    Task PublishAsync(string topic, string partitionKey, ReadOnlyMemory<byte> body, CancellationToken ct);

    Task SubscribeAsync(string topic, string consumerGroup,
        Func<ReadOnlyMemory<byte>, CancellationToken, Task> handler, CancellationToken ct);
}
