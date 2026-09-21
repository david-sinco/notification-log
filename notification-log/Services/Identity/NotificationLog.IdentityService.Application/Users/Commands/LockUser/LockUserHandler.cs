using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Application.Common;
using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Commands.LockUser;

public sealed class LockUserHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;
    private readonly ISessionRevoker _sessions;
    private readonly TimeProvider _time;

    public LockUserHandler(IUserRepository users, IUserSecurity security, ISessionRevoker sessions, TimeProvider time)
        => (_users, _security, _sessions, _time) = (users, security, sessions, time);

    public async Task HandleAsync(LockUserCommand cmd, ClaimsPrincipal principal, CancellationToken ct)
    {
        if (principal.GetUserId() == cmd.Id)
            throw ValidationError.For(nameof(cmd.Id), "No puedes bloquear tu propia cuenta.");

        if (cmd.Until is { } until && until <= _time.GetUtcNow())
            throw ValidationError.For(nameof(cmd.Until), "El bloqueo tiene que terminar en el futuro.");

        var user = await _users.FindByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("Usuario", cmd.Id);

        await _security.LockAsync(user, cmd.Until, ct);
        await _sessions.RevokeAllAsync(cmd.Id, ct);
    }
}
