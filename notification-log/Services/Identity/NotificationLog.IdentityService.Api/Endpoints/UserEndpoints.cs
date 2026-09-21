using System.Security.Claims;
using NotificationLog.IdentityService.Api.Connect;
using NotificationLog.IdentityService.Api.Contracts.Users;
using NotificationLog.IdentityService.Application.Users.Commands.LockUser;
using NotificationLog.IdentityService.Application.Users.Commands.SetUserRoles;
using NotificationLog.IdentityService.Application.Users.Commands.UnlockUser;
using NotificationLog.IdentityService.Application.Users.Dtos;
using NotificationLog.IdentityService.Application.Users.Queries.GetUserById;
using NotificationLog.IdentityService.Application.Users.Queries.SearchUsers;

namespace NotificationLog.IdentityService.Api.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUsers(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization(ConnectExtensions.IdentityScopePolicy);

        group.MapGet("/", SearchAsync)
            .RequireAuthorization(ConnectExtensions.AdministradorPolicy)
            .WithName("SearchUsers")
            .WithSummary("Busca usuarios por correo o teléfono")
            .Produces<IReadOnlyList<UserDto>>()
            .ProducesValidationProblem();

        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetUserById")
            .WithSummary("Obtiene el perfil propio, o el de cualquier usuario si eres administrador")
            .Produces<UserDto>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/roles", SetRolesAsync)
            .RequireAuthorization(ConnectExtensions.AdministradorPolicy)
            .WithName("SetUserRoles")
            .WithSummary("Reemplaza los roles de un usuario")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/lock", LockAsync)
            .RequireAuthorization(ConnectExtensions.AdministradorPolicy)
            .WithName("LockUser")
            .WithSummary("Bloquea un usuario y cierra todas sus sesiones")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/unlock", UnlockAsync)
            .RequireAuthorization(ConnectExtensions.AdministradorPolicy)
            .WithName("UnlockUser")
            .WithSummary("Desbloquea un usuario")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> SearchAsync(
        string? search,
        SearchUsersHandler handler,
        CancellationToken ct,
        int skip = 0,
        int take = 20)
        => Results.Ok(await handler.HandleAsync(new SearchUsersQuery(search, skip, take), ct));

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        ClaimsPrincipal user,
        GetUserByIdHandler handler,
        CancellationToken ct)
        => Results.Ok(await handler.HandleAsync(new GetUserByIdQuery(id), user, ct));

    private static async Task<IResult> SetRolesAsync(
        Guid id,
        SetUserRolesRequest body,
        ClaimsPrincipal user,
        SetUserRolesHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new SetUserRolesCommand(id, body.Roles), user, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> LockAsync(
        Guid id,
        LockUserRequest body,
        ClaimsPrincipal user,
        LockUserHandler handler,
        CancellationToken ct)
    {
        await handler.HandleAsync(new LockUserCommand(id, body.Until), user, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> UnlockAsync(Guid id, UnlockUserHandler handler, CancellationToken ct)
    {
        await handler.HandleAsync(new UnlockUserCommand(id), ct);

        return Results.NoContent();
    }
}
