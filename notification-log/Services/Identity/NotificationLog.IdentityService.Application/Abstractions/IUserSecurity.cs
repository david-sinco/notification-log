using NotificationLog.IdentityService.Application.Users.Common;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.Enums;

namespace NotificationLog.IdentityService.Application.Abstractions;

public interface IUserSecurity
{
    Task<AccountResult> SetPasswordAsync(User user, string? password, CancellationToken ct);

    Task<bool> CheckPasswordAsync(User user, string password, CancellationToken ct);

    Task<string> GenerateVerificationCodeAsync(User user, LoginChannel channel, CancellationToken ct);

    Task<bool> VerifyCodeAsync(User user, LoginChannel channel, string code, CancellationToken ct);

    Task<bool> IsLockedOutAsync(User user, CancellationToken ct);

    Task RegisterFailedAttemptAsync(User user, CancellationToken ct);

    Task ResetFailedAttemptsAsync(User user, CancellationToken ct);

    Task LockAsync(User user, DateTimeOffset? until, CancellationToken ct);

    Task UnlockAsync(User user, CancellationToken ct);

    Task SetRolesAsync(User user, IReadOnlyList<string> roles, CancellationToken ct);
}
