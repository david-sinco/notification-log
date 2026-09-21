using Application.Shared.Abstractions;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Application.Users.Common;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Application.Users.Commands.VerifyAccount;

public sealed class VerifyAccountHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;
    private readonly IUserEventPublisher _events;
    private readonly IUnitOfWork _uow;

    public VerifyAccountHandler(
        IUserRepository users, IUserSecurity security, IUserEventPublisher events, IUnitOfWork uow)
        => (_users, _security, _events, _uow) = (users, security, events, uow);

    public async Task<AccountResult> HandleAsync(VerifyAccountCommand cmd, CancellationToken ct)
    {
        var invalid = AccountResult.Fail("El código no es válido o ya venció.");

        var login = LoginIdentifier.TryParse(cmd.Identifier);
        var user = login is null ? null : await _users.FindByLoginAsync(login, ct);

        if (login is null || user is null)
            return invalid;

        if (user.IsConfirmed(login.Channel))
            return AccountResult.Ok();

        if (await _security.IsLockedOutAsync(user, ct))
            return AccountResult.Fail("Hiciste demasiados intentos. Intenta de nuevo más tarde.");

        if (!await _security.VerifyCodeAsync(user, login.Channel, cmd.Code.Trim(), ct))
        {
            await _security.RegisterFailedAttemptAsync(user, ct);

            return invalid;
        }

        var isNewAccount = !user.IsVerified;

        await _security.ResetFailedAttemptsAsync(user, ct);

        user.Confirm(login.Channel);

        await _users.UpdateAsync(user, ct);

        if (isNewAccount)
            await _events.PublishUserCreatedAsync(user, ct);
        else
            await _events.PublishVerificationChangedAsync(user, ct);

        await _uow.SaveChangesAsync(ct);

        return AccountResult.Ok();
    }
}
