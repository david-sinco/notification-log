using System.Collections.Concurrent;
using NotificationLog.RentalService.Application.Common.Producers;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class RecordingNotificationProducer : INotificationProducer
{
    private readonly ConcurrentQueue<SentNotification> _sent = new();

    public IReadOnlyList<SentNotification> Sent => _sent.ToList();

    public Task NotifyAsync(string eventKey, Guid recipientId, IReadOnlyDictionary<string, string> data, CancellationToken ct)
    {
        _sent.Enqueue(new SentNotification(eventKey, recipientId, new Dictionary<string, string>(data)));

        return Task.CompletedTask;
    }

    public void Clear() => _sent.Clear();
}
