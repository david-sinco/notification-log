using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Application.Users.Dtos;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Application.Users.Commands.SignIn;

public sealed class SignInHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;

    public SignInHandler(IUserRepository users, IUserSecurity security) => (_users, _security) = (users, security);

    public async Task<SignInAttempt> HandleAsync(SignInCommand cmd, CancellationToken ct)
    {
        var login = LoginIdentifier.TryParse(cmd.Identifier);
        var user = login is null ? null : await _users.FindByLoginAsync(login, ct);

        if (login is null || user is null)
            return SignInAttempt.Failed(SignInStatus.InvalidCredentials);

        if (await _security.IsLockedOutAsync(user, ct))
            return SignInAttempt.Failed(SignInStatus.LockedOut);

        if (!await _security.CheckPasswordAsync(user, cmd.Password, ct))
        {
            await _security.RegisterFailedAttemptAsync(user, ct);

            return SignInAttempt.Failed(await _security.IsLockedOutAsync(user, ct)
                ? SignInStatus.LockedOut
                : SignInStatus.InvalidCredentials);
        }

        await _security.ResetFailedAttemptsAsync(user, ct);

        if (!user.IsConfirmed(login.Channel))
            return SignInAttempt.Failed(SignInStatus.NotVerified);

        return new SignInAttempt(SignInStatus.Succeeded, SignedInUser.From(user));
    }
}
