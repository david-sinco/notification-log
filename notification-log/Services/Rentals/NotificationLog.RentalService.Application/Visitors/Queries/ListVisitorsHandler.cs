using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Visitors.Queries;

public sealed class ListVisitorsHandler(IVisitorReadModel visitors)
{
    private readonly IVisitorReadModel _visitors = visitors;

    public async Task<PagedResult<VisitorDto>> HandleAsync(PageRequest paging, CancellationToken ct)
    {
        var (items, total) = await _visitors.ListAsync(paging, ct);

        return new PagedResult<VisitorDto>(items, paging, total);
    }
}
