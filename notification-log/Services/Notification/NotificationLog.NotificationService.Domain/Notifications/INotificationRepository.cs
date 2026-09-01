namespace NotificationLog.NotificationService.Domain.Notifications;

public interface INotificationRepository
{
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> ListAsync(
        Guid? recipientId, string? eventKey, DeliveryStatus? status,
        int page, int pageSize, CancellationToken ct);
    Task AddAsync(Notification notification, CancellationToken ct);
}
