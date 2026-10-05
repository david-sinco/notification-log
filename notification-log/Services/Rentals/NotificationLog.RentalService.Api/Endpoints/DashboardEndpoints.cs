using System.Security.Claims;
using API.Shared.Extensions;
using NotificationLog.RentalService.Application.Dashboard.Queries;
using NotificationLog.RentalService.Application.Dashboard.Queries.Dtos;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboard(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard", GetAsync)
            .WithTags("Dashboard")
            .RequireAuthorization(AuthorizationExtensions.RentalsScopePolicy)
            .WithName("GetDashboard")
            .WithSummary("Resumen del panel: pendientes, contadores y publicaciones con actividad reciente")
            .Produces<DashboardDto>();

        return app;
    }

    private static async Task<IResult> GetAsync(
        ClaimsPrincipal user,
        GetDashboardHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(user, ct));
}
