using System.Security.Cryptography;
using System.Text;
using Notifications.Application.Ports;
using Notifications.Domain;

namespace Notifications.Infrastructure.Sending;

/// <summary>
/// Fails a deterministic slice of traffic — ~3% permanent, ~7% transient — so the retry and
/// discard paths actually get exercised during benchmarks (SPEC.md §8). Deterministic (hashed
/// from the idempotency key and attempt number, not RNG) so a benchmark run is reproducible.
/// </summary>
public sealed class FakeNotificationSender : INotificationSender
{
    public Task<SendResult> SendAsync(Notification notification, UserContact contact, CancellationToken ct)
    {
        var bucket = DeterministicBucket(notification.IdempotencyKey, notification.Attempts);

        SendResult result = bucket switch
        {
            < 3 => new SendResult(SendOutcome.PermanentFailure, "Simulated permanent failure (invalid recipient)."),
            < 10 => new SendResult(SendOutcome.TransientFailure, "Simulated transient failure (timeout)."),
            _ => new SendResult(SendOutcome.Sent),
        };

        return Task.FromResult(result);
    }

    /// <summary>Returns a stable value in [0, 100) for (key, attempt).</summary>
    private static int DeterministicBucket(string idempotencyKey, int attempt)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{idempotencyKey}:{attempt}"));
        return (int)(BitConverter.ToUInt32(hash, 0) % 100);
    }
}
