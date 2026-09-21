using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Application.Users.Commands.CreateAccount;

public sealed class CreateAccountHandler
{
    private readonly IUserRepository _users;
    private readonly IUserSecurity _security;
    private readonly IUserEventPublisher _events;
    private readonly IUnitOfWork _uow;

    public CreateAccountHandler(
        IUserRepository users, IUserSecurity security, IUserEventPublisher events, IUnitOfWork uow)
        => (_users, _security, _events, _uow) = (users, security, events, uow);

    public async Task<Guid> HandleAsync(CreateAccountCommand cmd, CancellationToken ct)
    {
        var login = LoginIdentifier.TryParse(cmd.Identifier)
            ?? throw new AppValidationException($"'{cmd.Identifier}' no es un correo ni un teléfono válido.");

        var locale = string.IsNullOrWhiteSpace(cmd.Locale) ? AccountPolicy.DefaultLocale : cmd.Locale;
        var timeZone = string.IsNullOrWhiteSpace(cmd.TimeZone) ? AccountPolicy.DefaultTimeZone : cmd.TimeZone;

        var user = await _users.FindByLoginAsync(login, ct);

        if (user is null)
        {
            if (cmd.Id == Guid.Empty)
                throw new AppValidationException("Hay que indicar el id de la cuenta.");

            if (await _users.FindByIdAsync(cmd.Id, ct) is not null)
                throw new AppValidationException($"Ya existe otra cuenta con el id '{cmd.Id}'.");

            user = User.Register(cmd.Id, login, cmd.Name, locale, timeZone, cmd.AcceptsNotifications);
            user.Confirm(login.Channel);

            await _users.AddAsync(user, ct);
        }
        else
        {
            user.UpdateProfile(cmd.Name, locale, timeZone, cmd.AcceptsNotifications);
            user.Confirm(login.Channel);

            await _users.UpdateAsync(user, ct);
        }

        if (cmd.Roles.Count > 0)
            await _security.SetRolesAsync(user, [.. cmd.Roles.Distinct().Select(role => role.ToString())], ct);

        await _events.PublishUserCreatedAsync(user, ct);
        await _uow.SaveChangesAsync(ct);

        return user.Id;
    }
}
