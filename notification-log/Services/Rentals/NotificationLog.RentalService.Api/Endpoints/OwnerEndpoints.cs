using System.Security.Claims;
using API.Shared.Extensions;
using Application.Shared.Pagination;
using NotificationLog.RentalService.Api.Contracts.Owners;
using NotificationLog.RentalService.Application.Owners.Commands.ClaimOwner;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterOwner;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.Filters;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class OwnerEndpoints
{
    public static IEndpointRouteBuilder MapOwners(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/owners")
            .WithTags("Owners")
            .RequireAuthorization(AuthorizationExtensions.RentalsScopePolicy);

        group.MapPost("/", RegisterAsync)
            .WithName("RegisterOwner")
            .WithSummary("Registra un propietario con su nombre, correo y teléfono")
            .Produces<CreatedOwnerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", ListAsync)
            .RequireAuthorization(AuthorizationExtensions.ModeracionPolicy)
            .WithName("ListOwners")
            .WithSummary("Lista todos los propietarios")
            .Produces<PagedResult<OwnerDto>>();

        group.MapPost("/{id:guid}/claim", ClaimAsync)
            .WithName("ClaimOwner")
            .WithSummary("Reclama un propietario sin usuario cuyo correo o teléfono coincide con el confirmado del usuario")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/mine", ListMineAsync)
            .WithName("ListMyOwners")
            .WithSummary("Lista los propietarios relacionados con el usuario")
            .Produces<IReadOnlyList<OwnerDto>>();

        group.MapGet("/claimable", ListClaimableAsync)
            .WithName("ListClaimableOwners")
            .WithSummary("Lista los propietarios sin usuario que coinciden con el correo o el teléfono confirmado del usuario")
            .Produces<IReadOnlyList<OwnerDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetOwnerById")
            .WithSummary("Obtiene un propietario tuyo o que registraste, o cualquiera si eres administrador o moderador")
            .Produces<OwnerDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> RegisterAsync(
        RegisterOwnerRequest body,
        ClaimsPrincipal user,
        RegisterOwnerHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(new RegisterOwnerCommand(body.Name, body.Email, body.Phone), user, ct);

        return Results.Created($"/api/owners/{id}", new CreatedOwnerResponse(id));
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] OwnerFilter filter,
        [AsParameters] PageRequest paging,
        ListOwnersHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(filter, paging, ct));

    private static async Task<IResult> ClaimAsync(
        Guid id,
        ClaimsPrincipal user,
        ClaimOwnerHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new ClaimOwnerCommand(id), user, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> ListMineAsync(
        ClaimsPrincipal user,
        ListMyOwnersHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(user, ct));

    private static async Task<IResult> ListClaimableAsync(
        ClaimsPrincipal user,
        ListClaimableOwnersHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(user, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        GetOwnerByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(id, user, ct));
}
