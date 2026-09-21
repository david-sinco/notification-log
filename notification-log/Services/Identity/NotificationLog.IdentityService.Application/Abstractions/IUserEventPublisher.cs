using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Application.Abstractions;

public interface IUserEventPublisher
{
    Task PublishUserCreatedAsync(User user, CancellationToken ct);

    Task PublishVerificationChangedAsync(User user, CancellationToken ct);

    Task PublishVerificationCodeRequestedAsync(LoginIdentifier login, string code, CancellationToken ct);
}
