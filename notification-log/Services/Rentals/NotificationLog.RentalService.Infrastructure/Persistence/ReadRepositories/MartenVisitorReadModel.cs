using NotificationLog.RentalService.Infrastructure.Persistence.Views;
using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

namespace NotificationLog.RentalService.Infrastructure.Persistence.ReadRepositories;

internal sealed class MartenVisitorReadModel : IVisitorReadModel
{
    private readonly IQuerySession _session;

    public MartenVisitorReadModel(IQuerySession session) => _session = session;

    public async Task<(IReadOnlyList<VisitorDto> Items, int TotalCount)> ListAsync(PageRequest paging, CancellationToken ct)
    {
        var items = await _session.Query<VisitorView>()
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.RegisteredAt)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        var counts = await VisitCountsAsync(items.Select(x => x.Id).ToArray(), ct);

        return (items.Select(x => ToDto(x, counts.GetValueOrDefault(x.Id))).ToList(), (int)stats.TotalResults);
    }

    public async Task<VisitorDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<VisitorView>(id, ct) is { } view
            ? ToDto(view, (await VisitCountsAsync([id], ct)).GetValueOrDefault(id))
            : null;

    private async Task<Dictionary<Guid, int>> VisitCountsAsync(Guid[] visitorIds, CancellationToken ct)
    {
        if (visitorIds.Length == 0)
            return [];

        var requested = await _session.Query<VisitView>()
            .Where(x => x.VisitorId.IsOneOf(visitorIds))
            .Select(x => x.VisitorId)
            .ToListAsync(ct);

        return requested.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
    }

    private static VisitorDto ToDto(VisitorView x, int visitCount) => new(
        x.Id,
        x.Status.ToString(),
        x.DisplayName,
        x.FirstNames,
        x.LastNames,
        x.DocumentType?.ToString(),
        x.DocumentNumber,
        x.Email,
        x.Phone,
        x.RegisteredAt,
        x.ProfileCompletedAt,
        visitCount);
}
