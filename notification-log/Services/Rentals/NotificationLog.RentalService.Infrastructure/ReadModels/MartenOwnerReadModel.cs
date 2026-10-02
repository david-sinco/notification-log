using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.Filters;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

internal sealed class MartenOwnerReadModel : IOwnerReadModel
{
    private readonly IQuerySession _session;

    public MartenOwnerReadModel(IQuerySession session) => _session = session;

    public async Task<(IReadOnlyList<OwnerDto> Items, int TotalCount)> ListAsync(OwnerFilter filter, PageRequest paging, CancellationToken ct)
    {
        IQueryable<OwnerView> owners = _session.Query<OwnerView>();

        if (filter.CreatedBy is { } createdBy)
            owners = owners.Where(x => x.CreatedBy == createdBy);

        if (filter.Type is { } type)
            owners = owners.Where(x => x.Type == type);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            owners = owners.Where(x =>
                x.FirstNames!.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.LastNames!.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.LegalName!.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.DocumentNumber!.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.Nit!.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        var items = await ((IMartenQueryable<OwnerView>)owners)
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.RegisteredAt)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        var counts = await ListingCountsAsync(items.Select(x => x.Id).ToArray(), ct);

        return (items.Select(x => ToDto(x, counts.GetValueOrDefault(x.Id))).ToList(), (int)stats.TotalResults);
    }

    public async Task<OwnerDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<OwnerView>(id, ct) is { } view
            ? ToDto(view, (await ListingCountsAsync([id], ct)).GetValueOrDefault(id))
            : null;

    private async Task<Dictionary<Guid, int>> ListingCountsAsync(Guid[] ownerIds, CancellationToken ct)
    {
        if (ownerIds.Length == 0)
            return [];

        var owned = await _session.Query<ListingView>()
            .Where(x => x.OwnerId.IsOneOf(ownerIds))
            .Select(x => x.OwnerId)
            .ToListAsync(ct);

        return owned.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
    }

    private static OwnerDto ToDto(OwnerView x, int listingCount) => new(
        x.Id,
        x.CreatedBy,
        x.Type.ToString(),
        x.FirstNames,
        x.LastNames,
        x.DocumentType?.ToString(),
        x.DocumentNumber,
        x.LegalName,
        x.Nit is null ? null : $"{x.Nit}-{x.NitCheckDigit}",
        x.Email,
        x.Phone,
        x.RegisteredAt,
        listingCount);
}
