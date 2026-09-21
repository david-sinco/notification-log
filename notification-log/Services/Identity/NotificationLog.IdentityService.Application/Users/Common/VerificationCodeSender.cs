using Application.Shared.Abstractions;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Application.Users.Common;

public sealed class VerificationCodeSender
{
    private readonly IUserSecurity _security;
    private readonly IUserEventPublisher _events;
    private readonly IUnitOfWork _uow;

    public VerificationCodeSender(IUserSecurity security, IUserEventPublisher events, IUnitOfWork uow)
        => (_security, _events, _uow) = (security, events, uow);

    public async Task SendAsync(User user, LoginIdentifier login, CancellationToken ct)
    {
        var code = await _security.GenerateVerificationCodeAsync(user, login.Channel, ct);

        await _events.PublishVerificationCodeRequestedAsync(login, code, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
