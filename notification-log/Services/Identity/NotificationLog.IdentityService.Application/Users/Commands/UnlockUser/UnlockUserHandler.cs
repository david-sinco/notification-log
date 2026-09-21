using Application.Shared.Common;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Commands.UnlockUser;

public sealed class UnlockUserHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;

    public UnlockUserHandler(IUserRepository users, IUserSecurity security) => (_users, _security) = (users, security);

    public async Task HandleAsync(UnlockUserCommand cmd, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException("Usuario", cmd.Id);

        await _security.UnlockAsync(user, ct);
    }
}
