using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Application.Common;
using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Commands.SetUserRoles;

public sealed class SetUserRolesHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;

    public SetUserRolesHandler(IUserRepository users, IUserSecurity security) => (_users, _security) = (users, security);

    public async Task HandleAsync(SetUserRolesCommand cmd, ClaimsPrincipal principal, CancellationToken ct)
    {
        if (cmd.Roles.Any(role => !Enum.IsDefined(role)))
            throw ValidationError.For(nameof(cmd.Roles), "Uno de los roles no es válido.");

        if (principal.GetUserId() == cmd.Id && !cmd.Roles.Contains(UserRole.Administrador))
            throw ValidationError.For(nameof(cmd.Roles), "No puedes quitarte el rol de administrador.");

        var user = await _users.FindByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("Usuario", cmd.Id);

        await _security.SetRolesAsync(user, [.. cmd.Roles.Distinct().Select(role => role.ToString())], ct);
    }
}
