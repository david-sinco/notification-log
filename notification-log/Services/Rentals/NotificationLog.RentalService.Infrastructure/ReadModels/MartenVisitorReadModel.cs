using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

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

        return (items.Select(ToDto).ToList(), (int)stats.TotalResults);
    }

    public async Task<VisitorDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<VisitorView>(id, ct) is { } view ? ToDto(view) : null;

    private static VisitorDto ToDto(VisitorView x) => new(
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
        x.ProfileCompletedAt);
}
