namespace NotificationLog.RentalService.Application.Common.Producers;

public interface INotificationProducer
{
    Task NotifyAsync(string eventKey, Guid recipientId, IReadOnlyDictionary<string, string> data, CancellationToken ct);
}
