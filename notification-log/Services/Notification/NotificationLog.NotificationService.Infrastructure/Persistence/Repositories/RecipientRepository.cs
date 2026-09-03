using Microsoft.EntityFrameworkCore;
using NotificationLog.NotificationService.Domain.Recipients;
using NotificationLog.NotificationService.Infrastructure.Persistence.Context;

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Repositories;

internal sealed class RecipientRepository(NotificationDbContext db) : IRecipientRepository
{
    private readonly NotificationDbContext _db = db;

    public Task<Recipient?> GetByIdAsync(Guid id, CancellationToken ct)
        => _db.Recipients.SingleOrDefaultAsync(r => r.Id == id, ct);

    public Task<bool> ExistsAsync(Guid id, CancellationToken ct)
        => _db.Recipients.AnyAsync(r => r.Id == id, ct);

    public async Task AddAsync(Recipient recipient, CancellationToken ct)
        => await _db.Recipients.AddAsync(recipient, ct);

    public async Task<(IReadOnlyList<Recipient> Items, int TotalCount)> ListAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Recipients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(r =>
                EF.Functions.Like(r.Name, pattern) ||
                (r.Email != null && EF.Functions.Like(r.Email, pattern)) ||
                (r.Phone != null && EF.Functions.Like(r.Phone, pattern)));
        }

        if (isActive.HasValue)
            query = query.Where(r => r.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(r => r.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }
}
