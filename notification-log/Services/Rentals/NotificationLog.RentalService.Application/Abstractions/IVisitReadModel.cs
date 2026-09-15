using NotificationLog.RentalService.Application.Visits.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.ListVisits;

namespace NotificationLog.RentalService.Application.Abstractions;

public interface IVisitReadModel
{
    Task<(IReadOnlyList<VisitSummaryDto> Items, int TotalCount)> ListAsync(ListVisitsQuery query, CancellationToken ct);

    Task<VisitDto?> GetAsync(Guid id, CancellationToken ct);
}
