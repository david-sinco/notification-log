using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotificationLog.IdentityService.Api.Accounts;
using NotificationLog.IdentityService.Api.Connect;
using NotificationLog.IdentityService.Api.Data;

namespace NotificationLog.IdentityService.Api.Users;

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
        IdentityServiceDbContext db,
        UserManager<ApplicationUser> users,
        CancellationToken ct,
        int skip = 0,
        int take = 20)
    {
        if (skip < 0 || take is < 1 or > AccountPolicy.MaxUsersPerPage)
            return Invalid("paginacion", $"Se pueden pedir entre 1 y {AccountPolicy.MaxUsersPerPage} usuarios desde una posición no negativa.");

        var query = db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var normalizedEmail = users.NormalizeEmail(term);

            query = query.Where(u =>
                (u.NormalizedEmail != null && u.NormalizedEmail.Contains(normalizedEmail))
                || (u.PhoneNumber != null && u.PhoneNumber.Contains(term)));
        }

        var page = await query
            .OrderBy(u => u.NormalizedEmail ?? u.PhoneNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        var ids = page.Select(u => u.Id).ToList();

        var roles = await (
                from userRole in db.UserRoles
                join role in db.Roles on userRole.RoleId equals role.Id
                where ids.Contains(userRole.UserId)
                select new { userRole.UserId, role.Name })
            .ToListAsync(ct);

        var rolesByUser = roles.ToLookup(r => r.UserId, r => r.Name!);

        return Results.Ok(page.Select(u => ToDto(u, [.. rolesByUser[u.Id]])).ToList());
    }

    private static async Task<IResult> GetByIdAsync(Guid id, ClaimsPrincipal principal, UserManager<ApplicationUser> users)
    {
        if (principal.GetUserId() != id && !principal.IsInRole(nameof(UserRole.Administrador)))
            return Forbidden("Solo puedes consultar tu propio perfil.");

        var user = await users.FindByIdAsync(id.ToString());

        return user is null
            ? UserNotFound(id)
            : Results.Ok(ToDto(user, [.. await users.GetRolesAsync(user)]));
    }

    private static async Task<IResult> SetRolesAsync(
        Guid id,
        SetUserRolesRequest body,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> users)
    {
        if (body.Roles.Any(role => !Enum.IsDefined(role)))
            return Invalid(nameof(body.Roles), "Uno de los roles no es válido.");

        if (principal.GetUserId() == id && !body.Roles.Contains(UserRole.Administrador))
            return Invalid(nameof(body.Roles), "No puedes quitarte el rol de administrador.");

        var user = await users.FindByIdAsync(id.ToString());

        if (user is null)
            return UserNotFound(id);

        var current = await users.GetRolesAsync(user);
        var requested = body.Roles.Distinct().Select(role => role.ToString()).ToList();

        await users.RemoveFromRolesAsync(user, current.Except(requested));
        await users.AddToRolesAsync(user, requested.Except(current));

        return Results.NoContent();
    }

    private static async Task<IResult> LockAsync(
        Guid id,
        LockUserRequest body,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> users,
        SessionRevoker sessions,
        TimeProvider time,
        CancellationToken ct)
    {
        if (principal.GetUserId() == id)
            return Invalid(nameof(id), "No puedes bloquear tu propia cuenta.");

        if (body.Until is { } until && until <= time.GetUtcNow())
            return Invalid(nameof(body.Until), "El bloqueo tiene que terminar en el futuro.");

        var user = await users.FindByIdAsync(id.ToString());

        if (user is null)
            return UserNotFound(id);

        await users.SetLockoutEnabledAsync(user, true);
        await users.SetLockoutEndDateAsync(user, body.Until ?? DateTimeOffset.MaxValue);
        await users.UpdateSecurityStampAsync(user);
        await sessions.RevokeAllAsync(id, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> UnlockAsync(Guid id, UserManager<ApplicationUser> users)
    {
        var user = await users.FindByIdAsync(id.ToString());

        if (user is null)
            return UserNotFound(id);

        await users.SetLockoutEndDateAsync(user, null);
        await users.ResetAccessFailedCountAsync(user);

        return Results.NoContent();
    }

    private static UserDto ToDto(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(user.Id,
            user.Email,
            user.EmailConfirmed,
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            user.LockoutEnd > DateTimeOffset.UtcNow ? user.LockoutEnd : null,
            roles);

    private static IResult Invalid(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] }, title: "Errores de validación");

    private static IResult Forbidden(string detail) =>
        Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Acceso denegado", detail: detail);

    private static IResult UserNotFound(Guid id) =>
        Results.Problem(statusCode: StatusCodes.Status404NotFound, title: "Recurso no encontrado", detail: $"Usuario con id '{id}' no fue encontrado.");
}
