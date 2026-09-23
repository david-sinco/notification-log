using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Filters;

namespace NotificationLog.RentalService.Application.Visits.Queries;

public interface IVisitReadModel
{
    Task<(IReadOnlyList<VisitSummaryDto> Items, int TotalCount)> ListAsync(VisitFilter filter, PageRequest paging, CancellationToken ct);

    Task<VisitDto?> GetAsync(Guid id, CancellationToken ct);
}
