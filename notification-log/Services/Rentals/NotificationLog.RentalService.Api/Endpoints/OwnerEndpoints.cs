using System.Security.Claims;
using API.Shared.Extensions;
using NotificationLog.RentalService.Api.Contracts.Owners;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;
using NotificationLog.RentalService.Application.Owners.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.GetOwnerById;
using NotificationLog.RentalService.Application.Owners.Queries.ListOwners;

namespace NotificationLog.RentalService.Api.Endpoints;

public static class OwnerEndpoints
{
    public static IEndpointRouteBuilder MapOwners(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/owners")
            .WithTags("Owners")
            .RequireAuthorization(AuthorizationExtensions.RentalsScopePolicy);

        group.MapPost("/natural", RegisterNaturalAsync)
            .WithName("RegisterNaturalOwner")
            .WithSummary("Registra un propietario persona natural")
            .Produces<CreatedOwnerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/company", RegisterCompanyAsync)
            .WithName("RegisterCompanyOwner")
            .WithSummary("Registra un propietario persona jurídica")
            .Produces<CreatedOwnerResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/", ListAsync)
            .RequireAuthorization(AuthorizationExtensions.ModeracionPolicy)
            .WithName("ListOwners")
            .WithSummary("Lista todos los propietarios")
            .Produces<PagedResult<OwnerDto>>();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetOwnerById")
            .WithSummary("Obtiene un propietario que registraste, o cualquiera si eres administrador o moderador")
            .Produces<OwnerDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> RegisterNaturalAsync(
        RegisterNaturalOwnerRequest body,
        ClaimsPrincipal user,
        RegisterNaturalOwnerHandler handler,
        CancellationToken ct)
    {
        var command = new RegisterNaturalOwnerCommand(
            body.FirstNames, body.LastNames, body.DocumentType, body.DocumentNumber, body.Email, body.Phone);
        var id = await handler.HandleAsync(command, user, ct);

        return Results.Created($"/api/owners/{id}", new CreatedOwnerResponse(id));
    }

    private static async Task<IResult> RegisterCompanyAsync(
        RegisterCompanyOwnerRequest body,
        ClaimsPrincipal user,
        RegisterCompanyOwnerHandler handler,
        CancellationToken ct)
    {
        var id = await handler.HandleAsync(new RegisterCompanyOwnerCommand(body.LegalName, body.Nit, body.Email, body.Phone), user, ct);

        return Results.Created($"/api/owners/{id}", new CreatedOwnerResponse(id));
    }

    private static async Task<IResult> ListAsync(
        [AsParameters] ListOwnersQuery query,
        ListOwnersHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(query, ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        GetOwnerByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetOwnerByIdQuery(id), user, ct));
}
