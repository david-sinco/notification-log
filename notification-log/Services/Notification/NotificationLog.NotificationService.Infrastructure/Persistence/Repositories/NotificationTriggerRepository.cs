using Microsoft.EntityFrameworkCore;
using NotificationLog.NotificationService.Domain.Triggers;
using NotificationLog.NotificationService.Infrastructure.Persistence.Context;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Repositories;

internal sealed class NotificationTriggerRepository(NotificationDbContext db) : INotificationTriggerRepository
{
    private readonly NotificationDbContext _db = db;

    public Task<NotificationTrigger?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Triggers
            .Include(t => t.Configurations)
            .SingleOrDefaultAsync(t => t.Id == id, ct);

    public Task<NotificationTrigger?> GetByEventKeyAsync(EventKey eventKey, CancellationToken ct)
        => _db.Triggers
            .Include(t => t.Configurations)
            .SingleOrDefaultAsync(t => t.EventKey == eventKey, ct);

    public Task<bool> ExistsAsync(EventKey eventKey, CancellationToken ct)
        => _db.Triggers.AnyAsync(t => t.EventKey == eventKey, ct);

    public async Task AddAsync(NotificationTrigger trigger, CancellationToken ct)
        => await _db.Triggers.AddAsync(trigger, ct);

    public async Task<(IReadOnlyList<NotificationTrigger> Items, int TotalCount)> ListAsync(
        string? search, bool? isEnabled, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Triggers
            .Include(t => t.Configurations)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(t =>
                EF.Functions.Like(EF.Property<string>(t, "EventKey"), pattern) ||
                EF.Functions.Like(t.Description, pattern));
        }

        if (isEnabled.HasValue)
            query = query.Where(t => t.IsEnabled == isEnabled.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => EF.Property<string>(t, "EventKey"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
