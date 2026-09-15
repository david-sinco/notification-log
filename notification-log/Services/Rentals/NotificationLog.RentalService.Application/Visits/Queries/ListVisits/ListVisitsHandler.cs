using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;

namespace NotificationLog.RentalService.Application.Visits.Queries.ListVisits;

public sealed class ListVisitsHandler
{
    private readonly IVisitReadModel _visits;

    public ListVisitsHandler(IVisitReadModel visits) => _visits = visits;

    public async Task<PagedResult<VisitSummaryDto>> HandleAsync(ListVisitsQuery query, CancellationToken ct)
    {
        var paged = query with { Page = Paging.Page(query.Page), PageSize = Paging.Size(query.PageSize) };
        var (items, total) = await _visits.ListAsync(paged, ct);

        return new PagedResult<VisitSummaryDto>(items, paged.Page, paged.PageSize, total);
    }
}
