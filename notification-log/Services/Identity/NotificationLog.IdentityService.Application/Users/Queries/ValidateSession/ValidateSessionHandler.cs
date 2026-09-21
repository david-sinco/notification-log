using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Application.Users.Dtos;
using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Queries.ValidateSession;

public sealed class ValidateSessionHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;

    public ValidateSessionHandler(IUserRepository users, IUserSecurity security)
        => (_users, _security) = (users, security);

    public async Task<SignedInUser?> HandleAsync(ValidateSessionQuery query, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(query.UserId, ct);

        if (user is null
            || !user.IsVerified
            || user.SecurityStamp != query.SecurityStamp
            || await _security.IsLockedOutAsync(user, ct))
            return null;

        return SignedInUser.From(user);
    }
}
