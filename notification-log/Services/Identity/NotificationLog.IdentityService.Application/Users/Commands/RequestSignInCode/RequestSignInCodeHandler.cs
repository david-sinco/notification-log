using NotificationLog.IdentityService.Application.Users.Common;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Application.Users.Commands.RequestSignInCode;

public sealed class RequestSignInCodeHandler
{
    private readonly IUserRepository _users;
    private readonly VerificationCodeSender _codes;

    public RequestSignInCodeHandler(IUserRepository users, VerificationCodeSender codes)
        => (_users, _codes) = (users, codes);

    public async Task HandleAsync(RequestSignInCodeCommand cmd, CancellationToken ct)
    {
        var login = LoginIdentifier.TryParse(cmd.Identifier);
        var user = login is null ? null : await _users.FindByLoginAsync(login, ct);

        if (login is null || user is null)
            return;

        await _codes.SendAsync(user, login, ct);
    }
}
