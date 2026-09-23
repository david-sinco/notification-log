using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Abstractions;

namespace NotificationLog.RentalService.Application.Visits.Queries.ListVisits;

public sealed class ListVisitsHandler
{
    private readonly IVisitReadModel _visits;

    public ListVisitsHandler(IVisitReadModel visits) => _visits = visits;

    public async Task<PagedResult<VisitSummaryDto>> HandleAsync(ListVisitsQuery query, PageRequest paging, CancellationToken ct)
    {
        var (items, total) = await _visits.ListAsync(query, paging, ct);

        return new PagedResult<VisitSummaryDto>(items, paging, total);
    }
}
