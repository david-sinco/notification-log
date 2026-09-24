using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Application.Common;
using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Commands.SetUserRole;

public sealed class SetUserRoleHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;

    public SetUserRoleHandler(IUserRepository users, IUserSecurity security) => (_users, _security) = (users, security);

    public async Task HandleAsync(SetUserRoleCommand cmd, ClaimsPrincipal principal, CancellationToken ct)
    {
        if (!Enum.IsDefined(cmd.Role))
            throw ValidationError.For(nameof(cmd.Role), "El rol no es válido.");

        if (principal.GetUserId() == cmd.Id && cmd.Role != UserRole.Administrador)
            throw ValidationError.For(nameof(cmd.Role), "No puedes quitarte el rol de administrador.");

        var user = await _users.FindByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("Usuario", cmd.Id);

        await _security.SetRoleAsync(user, cmd.Role, ct);
    }
}
