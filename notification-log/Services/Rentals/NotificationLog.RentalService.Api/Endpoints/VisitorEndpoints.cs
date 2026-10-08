using System.Security.Claims;
using API.Shared.Extensions;
using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class VisitorEndpoints
{
    public static IEndpointRouteBuilder MapVisitors(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/visitors")
            .WithTags("Visitors")
            .RequireAuthorization(AuthorizationExtensions.RentalsScopePolicy);

        group.MapGet("/", ListAsync)
            .RequireAuthorization(AuthorizationExtensions.ModeracionPolicy)
            .WithName("ListVisitors")
            .WithSummary("Lista todos los visitantes")
            .Produces<PagedResult<VisitorDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetVisitorById")
            .WithSummary("Obtiene tu registro de visitante, o cualquiera si eres administrador o moderador")
            .Produces<VisitorDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] PageRequest paging,
        ListVisitorsHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(paging, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        GetVisitorByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(id, user, ct));
}
