using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Filters;

namespace NotificationLog.RentalService.Application.Visits.Queries;

public sealed class ListVisitsHandler
{
    private readonly IVisitReadModel _visits;

    public ListVisitsHandler(IVisitReadModel visits) => _visits = visits;

    public async Task<PagedResult<VisitSummaryDto>> HandleAsync(VisitFilter filter, PageRequest paging, CancellationToken ct)
    {
        var (items, total) = await _visits.ListAsync(filter, paging, ct);

        return new PagedResult<VisitSummaryDto>(items, paging, total);
    }
}
