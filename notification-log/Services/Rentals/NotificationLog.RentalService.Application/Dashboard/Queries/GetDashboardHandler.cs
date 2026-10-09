using System.Security.Claims;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Application.Dashboard.Queries.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries;

namespace NotificationLog.RentalService.Application.Dashboard.Queries;

public sealed class GetDashboardHandler(IDashboardReadModel dashboard, IPhotoUrlProvider photoUrls)
{
    private readonly IDashboardReadModel _dashboard = dashboard;
    private readonly IPhotoUrlProvider _photoUrls = photoUrls;

    public async Task<DashboardDto> HandleAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        var dashboard = await _dashboard.GetAsync(user.GetUserId(), user.IsAdministrador() || user.IsModerador(), ct);

        return dashboard with { RecentListings = [.. dashboard.RecentListings.Select(x => x.WithCoverUrl(_photoUrls))] };
    }
}
