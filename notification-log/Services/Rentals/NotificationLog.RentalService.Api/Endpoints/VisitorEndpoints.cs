 using System.Security.Claims;
using API.Shared.Extensions;
using Application.Shared.Pagination;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Api.Contracts.Visitors;
using NotificationLog.RentalService.Application.Visitors.Commands.CompleteVisitorProfile;
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

        group.MapGet("/me", GetMeAsync)
            .WithName("GetMyVisitor")
            .WithSummary("Obtiene el registro de visitante del usuario autenticado")
            .Produces<VisitorDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/me/profile", CompleteProfileAsync)
            .WithName("CompleteVisitorProfile")
            .WithSummary("Registra al usuario autenticado como visitante con sus datos completos")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

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

    private static async Task<IResult> GetMeAsync(
        ClaimsPrincipal user,
        GetVisitorByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(user.GetUserId(), user, ct));

    private static async Task<IResult> CompleteProfileAsync(
        CompleteVisitorProfileRequest body,
        ClaimsPrincipal user,
        CompleteVisitorProfileHandler handler,
        CancellationToken ct)
    {
        var command = new CompleteVisitorProfileCommand(
            body.FirstNames, body.LastNames, body.DocumentType, body.DocumentNumber, body.Email, body.Phone);

        await handler.HandleAsync(command, user, ct);

        return Results.NoContent();
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
