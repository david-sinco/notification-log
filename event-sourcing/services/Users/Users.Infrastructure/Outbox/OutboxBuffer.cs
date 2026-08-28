using System.Text.Json;
using Users.Application.Ports;

namespace Users.Infrastructure.Outbox;

/// <summary>
/// Scoped buffer implementing Users.Application's IIntegrationEventOutbox port. Handlers enqueue
/// onto it during a request; UserRepository drains it into OutboxMessage rows on the same Marten
/// session immediately before SaveChangesAsync, so the append, the reservation, and the outbox
/// row all commit atomically (SPEC.md §6).
/// </summary>
public sealed class OutboxBuffer : IIntegrationEventOutbox
{
    private readonly List<(string Topic, string PartitionKey, object Payload)> _pending = [];

    public void Enqueue(string topic, string partitionKey, object payload) =>
        _pending.Add((topic, partitionKey, payload));

    public IReadOnlyList<OutboxMessage> DrainAsMessages(DateTimeOffset now)
    {
        if (_pending.Count == 0)
            return [];

        var messages = _pending.Select(p => new OutboxMessage
        {
            Topic = p.Topic,
            PartitionKey = p.PartitionKey,
            PayloadJson = JsonSerializer.Serialize(p.Payload, p.Payload.GetType()),
            CreatedAt = now,
        }).ToArray();

        _pending.Clear();
        return messages;
    }
}
