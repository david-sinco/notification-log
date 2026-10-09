using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;
using NotificationLog.RentalService.Application.Visitors.Queries.Filters;

namespace NotificationLog.RentalService.Application.Visitors.Queries;

public interface IVisitorReadModel
{
    Task<(IReadOnlyList<VisitorDto> Items, int TotalCount)> ListAsync(VisitorFilter filter, PageRequest paging, CancellationToken ct);

    Task<VisitorDetailDto?> GetAsync(Guid id, CancellationToken ct);

    Task<VisitorsSummaryDto> GetSummaryAsync(DateTimeOffset registeredSince, CancellationToken ct);
}
