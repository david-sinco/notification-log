using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;
using NotificationLog.RentalService.Application.Visitors.Queries.Filters;

namespace NotificationLog.RentalService.Application.Visitors.Queries;

public sealed class ListVisitorsHandler(IVisitorReadModel visitors)
{
    private readonly IVisitorReadModel _visitors = visitors;

    public async Task<PagedResult<VisitorDto>> HandleAsync(VisitorFilter filter, PageRequest paging, CancellationToken ct)
    {
        var (items, total) = await _visitors.ListAsync(filter, paging, ct);

        return new PagedResult<VisitorDto>(items, paging, total);
    }
}
