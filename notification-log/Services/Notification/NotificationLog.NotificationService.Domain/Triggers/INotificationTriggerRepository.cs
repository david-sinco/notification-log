namespace NotificationLog.NotificationService.Domain.Triggers;

public interface INotificationTriggerRepository
{
    Task<NotificationTrigger?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<NotificationTrigger?> GetByEventKeyAsync(EventKey eventKey, CancellationToken ct);
    Task<bool> ExistsAsync(EventKey eventKey, CancellationToken ct);
    Task AddAsync(NotificationTrigger trigger, CancellationToken ct);
    Task<(IReadOnlyList<NotificationTrigger> Items, int TotalCount)> ListAsync(string? search, bool? isEnabled, int page, int pageSize, CancellationToken ct);
}
