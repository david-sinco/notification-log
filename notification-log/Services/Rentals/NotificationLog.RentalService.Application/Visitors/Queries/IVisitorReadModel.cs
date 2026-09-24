using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Visitors.Queries;

public interface IVisitorReadModel
{
    Task<(IReadOnlyList<VisitorDto> Items, int TotalCount)> ListAsync(PageRequest paging, CancellationToken ct);

    Task<VisitorDto?> GetAsync(Guid id, CancellationToken ct);
}
