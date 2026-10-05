using NotificationLog.RentalService.Application.Dashboard.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Dashboard.Queries;

public interface IDashboardReadModel
{
    Task<DashboardDto> GetAsync(Guid userId, bool isStaff, CancellationToken ct);
}
