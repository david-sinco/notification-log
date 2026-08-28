using Notifications.Domain;

namespace Notifications.Application.Ports;

public enum SendOutcome { Sent, TransientFailure, PermanentFailure }

public sealed record SendResult(SendOutcome Outcome, string? Error = null);

/// <summary>
/// Notifications.Infrastructure provides a fake implementation that fails a deterministic slice
/// of traffic (~3% permanent, ~7% transient) so the retry and discard paths actually get
/// exercised during benchmarks (SPEC.md §8).
/// </summary>
public interface INotificationSender
{
    Task<SendResult> SendAsync(Notification notification, UserContact contact, CancellationToken ct);
}
