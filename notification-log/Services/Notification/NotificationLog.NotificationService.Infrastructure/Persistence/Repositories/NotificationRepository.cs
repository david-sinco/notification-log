using Microsoft.EntityFrameworkCore;
using NotificationLog.NotificationService.Domain.Notifications;
using NotificationLog.NotificationService.Infrastructure.Persistence.Context;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Repositories;

internal sealed class NotificationRepository(NotificationDbContext db) : INotificationRepository
{
    private readonly NotificationDbContext _db = db;

    public async Task AddAsync(Notification notification, CancellationToken ct)
        => await _db.Notifications.AddAsync(notification, ct);

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> ListAsync(
        Guid? recipientId, string? eventKey, DeliveryStatus? status,
        int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Notifications.AsNoTracking();

        if (recipientId.HasValue)
            query = query.Where(n => n.RecipientId == recipientId.Value);

        if (!string.IsNullOrWhiteSpace(eventKey))
            query = query.Where(n => n.EventKey == eventKey.Trim());

        if (status.HasValue)
            query = query.Where(n => n.Status == status.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(n => n.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
