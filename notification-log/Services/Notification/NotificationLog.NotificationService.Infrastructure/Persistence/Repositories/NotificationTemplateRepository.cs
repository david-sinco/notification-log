using Microsoft.EntityFrameworkCore;
using NotificationLog.NotificationService.Domain.Shared;
using NotificationLog.NotificationService.Domain.Templates;
using NotificationLog.NotificationService.Infrastructure.Persistence.Context;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Repositories;

internal sealed class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly NotificationDbContext _db;

    public NotificationTemplateRepository(NotificationDbContext db) => _db = db;

    public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Templates
            .Include(t => t.Versions)
            .SingleOrDefaultAsync(t => t.Id == id, ct);

    public Task<bool> ExistsAsync(TemplateName name, CancellationToken ct)
        => _db.Templates.AnyAsync(t => t.Name == name, ct);

    public async Task AddAsync(NotificationTemplate template, CancellationToken ct)
        => await _db.Templates.AddAsync(template, ct);

    public async Task<(IReadOnlyList<NotificationTemplate> Items, int TotalCount)> ListAsync(
        string? search, NotificationChannel? channel, bool? isEnabled,
        int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Templates
            .Include(t => t.Versions)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(t => EF.Functions.Like(EF.Property<string>(t, "Name"), pattern));
        }

        if (channel.HasValue)
            query = query.Where(t => t.Channel == channel.Value);

        if (isEnabled.HasValue)
            query = query.Where(t => t.IsEnabled == isEnabled.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(t => EF.Property<string>(t, "Name"))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
