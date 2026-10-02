using NotificationLog.NotificationService.Domain.Shared;
namespace NotificationLog.NotificationService.Domain.Notifications;

public interface INotificationRepository
{
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> ListAsync(
        Guid? recipientId, string? eventKey, DeliveryStatus? status,
        NotificationChannel? channel, string? search,
        int page, int pageSize, CancellationToken ct);
    Task AddAsync(Notification notification, CancellationToken ct);
}
