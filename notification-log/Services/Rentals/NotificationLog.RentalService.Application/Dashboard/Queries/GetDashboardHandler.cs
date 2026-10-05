using System.Security.Claims;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Dashboard.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Dashboard.Queries;

public sealed class GetDashboardHandler(IDashboardReadModel dashboard)
{
    private readonly IDashboardReadModel _dashboard = dashboard;

    public Task<DashboardDto> HandleAsync(ClaimsPrincipal user, CancellationToken ct)
        => _dashboard.GetAsync(user.GetUserId(), user.IsAdministrador() || user.IsModerador(), ct);
}
