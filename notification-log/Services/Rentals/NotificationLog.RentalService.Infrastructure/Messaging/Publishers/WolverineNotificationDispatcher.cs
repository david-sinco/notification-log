using Google.Protobuf.WellKnownTypes;
using NotificationLog.Contracts.Notifications;
using NotificationLog.RentalService.Application.Abstractions;
using Wolverine;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Publishers;

internal sealed class WolverineNotificationDispatcher : INotificationDispatcher
{
    private readonly IMessageBus _bus;
    private readonly TimeProvider _time;

    public WolverineNotificationDispatcher(IMessageBus bus, TimeProvider time) => (_bus, _time) = (bus, time);

    public async Task DispatchAsync(string eventKey, Guid recipientId, IReadOnlyDictionary<string, string> data, CancellationToken ct)
    {
        var message = new NotificationDispatchRequested
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTimeOffset(_time.GetUtcNow()),
            SchemaVersion = 1,
            EventKey = eventKey,
            RecipientId = recipientId.ToString()
        };

        foreach (var (key, value) in data)
            message.Data[key] = value;

        await _bus.PublishAsync(message);
    }
}
