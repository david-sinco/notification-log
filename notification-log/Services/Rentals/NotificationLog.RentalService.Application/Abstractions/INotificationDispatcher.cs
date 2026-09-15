namespace NotificationLog.RentalService.Application.Abstractions;

public interface INotificationDispatcher
{
    Task DispatchAsync(string eventKey, Guid recipientId, IReadOnlyDictionary<string, string> data, CancellationToken ct);
}
