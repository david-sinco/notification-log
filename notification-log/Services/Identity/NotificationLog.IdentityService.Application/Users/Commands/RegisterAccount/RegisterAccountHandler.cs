using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Application.Users.Common;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Application.Users.Commands.RegisterAccount;

public sealed class RegisterAccountHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;
    private readonly VerificationCodeSender _codes;

    public RegisterAccountHandler(IUserRepository users, IUserSecurity security, VerificationCodeSender codes)
        => (_users, _security, _codes) = (users, security, codes);

    public async Task<AccountResult> HandleAsync(RegisterAccountCommand cmd, CancellationToken ct)
    {
        var login = LoginIdentifier.TryParse(cmd.Identifier);

        if (login is null)
            return AccountResult.Fail("Escribe un correo o un teléfono válido.");

        var name = cmd.Name?.Trim() ?? string.Empty;

        if (Validate(name, cmd) is { } error)
            return AccountResult.Fail(error);

        var user = await _users.FindByLoginAsync(login, ct);

        if (user is { IsVerified: true })
            return AccountResult.Fail("Ya existe una cuenta con ese correo o teléfono.");

        if (user is null)
        {
            user = User.Register(Guid.NewGuid(), login, name, cmd.Locale, cmd.TimeZone, cmd.AcceptsNotifications);

            await _users.AddAsync(user, ct);
        }
        else
        {
            user.UpdateProfile(name, cmd.Locale, cmd.TimeZone, cmd.AcceptsNotifications);

            await _users.UpdateAsync(user, ct);
        }

        var password = await _security.SetPasswordAsync(user, cmd.Password, ct);

        if (!password.Succeeded)
            return password;

        await _codes.SendAsync(user, login, ct);

        return AccountResult.Ok();
    }

    private static string? Validate(string name, RegisterAccountCommand cmd) =>
        AccountPolicy.ValidateName(name)
        ?? AccountPolicy.ValidateLocale(cmd.Locale)
        ?? AccountPolicy.ValidateTimeZone(cmd.TimeZone)
        ?? AccountPolicy.ValidateConsent(cmd.AcceptsNotifications)
        ?? AccountPolicy.ValidatePassword(cmd.Password);
}
