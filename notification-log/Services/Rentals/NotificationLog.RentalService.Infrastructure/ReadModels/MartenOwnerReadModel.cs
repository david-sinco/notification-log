using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.Filters;
using NotificationLog.RentalService.Domain.Owners.Enums;

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

        var items = await ((IMartenQueryable<OwnerView>)owners)
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.RegisteredAt)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return (items.Select(ToDto).ToList(), (int)stats.TotalResults);
    }

    public async Task<OwnerDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<OwnerView>(id, ct) is { } view ? ToDto(view) : null;

    public Task<bool> ExistsWithDocumentAsync(DocumentType type, string number, CancellationToken ct)
        => _session.Query<OwnerView>().AnyAsync(x => x.DocumentType == type && x.DocumentNumber == number, ct);

    public Task<bool> ExistsWithNitAsync(string nit, CancellationToken ct)
        => _session.Query<OwnerView>().AnyAsync(x => x.Nit == nit, ct);

    private static OwnerDto ToDto(OwnerView x) => new(
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
        x.RegisteredAt);
}
