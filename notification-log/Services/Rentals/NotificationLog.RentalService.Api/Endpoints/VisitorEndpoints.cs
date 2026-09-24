using System.Security.Claims;
using API.Shared.Extensions;
using Application.Shared.Pagination;
using NotificationLog.RentalService.Api.Contracts.Visitors;
using NotificationLog.RentalService.Application.Visitors.Commands.RegisterVisitor;
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

        group.MapPost("/", RegisterAsync)
            .WithName("RegisterVisitor")
            .WithSummary("Registra un visitante y solicita la creación de su cuenta")
            .Produces<CreatedVisitorResponse>(StatusCodes.Status201Created)
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

    private static async Task<IResult> RegisterAsync(
        RegisterVisitorRequest body,
        ClaimsPrincipal user,
        RegisterVisitorHandler handler,
        CancellationToken ct)
    {
        var command = new RegisterVisitorCommand(
            body.FirstNames, body.LastNames, body.DocumentType, body.DocumentNumber, body.Email, body.Phone);
        var id = await handler.HandleAsync(command, user, ct);

        return Results.Created($"/api/visitors/{id}", new CreatedVisitorResponse(id));
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
