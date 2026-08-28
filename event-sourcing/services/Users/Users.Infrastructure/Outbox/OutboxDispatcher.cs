using System.Text;
using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shared.Messaging;

namespace Users.Infrastructure.Outbox;

/// <summary>
/// Polls undispatched outbox rows, publishes them, marks them dispatched (SPEC.md §6).
/// </summary>
public sealed class OutboxDispatcher(
    IDocumentStore store, IMessageTransport transport, ILogger<OutboxDispatcher> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatch loop failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task DispatchPendingAsync(CancellationToken ct)
    {
        await using var session = store.LightweightSession();

        // Poll on DispatchedAt == null, never on a sequence cursor (SPEC.md §6): sequence
        // numbers are assigned at insert time but rows become visible at commit time, so a
        // transaction holding a higher sequence can commit before one holding a lower one. A
        // poller reading "seq > lastSeen" can then skip the still-invisible lower row forever,
        // silently. Marking rows dispatched sidesteps this entirely.
        var pending = await session.Query<OutboxMessage>()
            .Where(m => m.DispatchedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(200)
            .ToListAsync(ct);

        if (pending.Count == 0)
            return;

        // Only the oldest undispatched row per key goes out this pass. Without this, a message
        // that fails here and a later message for the *same* key that succeeds on its first try
        // would reach the transport out of order — harmless for classic RabbitMQ (each publish is
        // independent and the consumer-side version check in ContactReplicationService absorbs
        // it), but wrong for a transport that preserves send order as the actual log (RabbitMQ
        // Streams' single stream, Kafka's per-partition log): whichever arrives first there stays
        // first, permanently. Other keys still make progress in the same pass; a stuck key just
        // falls behind — it can never let a later message for itself overtake it.
        var headPerKey = pending
            .GroupBy(m => m.PartitionKey)
            .Select(g => g.OrderBy(m => m.CreatedAt).First());

        foreach (var message in headPerKey)
        {
            try
            {
                await transport.PublishAsync(
                    message.Topic, message.PartitionKey, Encoding.UTF8.GetBytes(message.PayloadJson), ct);

                message.DispatchedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                message.Attempts++;
                logger.LogWarning(ex,
                    "Failed to dispatch outbox message {MessageId} (attempt {Attempts})",
                    message.Id, message.Attempts);
            }

            session.Store(message);
        }

        await session.SaveChangesAsync(ct);
    }
}
