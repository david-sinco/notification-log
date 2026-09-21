using Domain.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Application.Users.Common;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.Enums;
using NotificationLog.IdentityService.Infrastructure.Persistence;

namespace NotificationLog.IdentityService.Infrastructure.Security;

internal sealed class IdentityUserSecurity : IUserSecurity
{
    private const string VerificationPurpose = "VerifyAccount";

    private readonly UserManager<ApplicationUser> _users;

    public IdentityUserSecurity(UserManager<ApplicationUser> users) => _users = users;

    public async Task<AccountResult> SetPasswordAsync(User user, string? password, CancellationToken ct)
    {
        var entity = await RequireAsync(user);

        await _users.RemovePasswordAsync(entity);

        if (string.IsNullOrEmpty(password))
            return AccountResult.Ok();

        var result = await _users.AddPasswordAsync(entity, password);

        return result.Succeeded
            ? AccountResult.Ok()
            : AccountResult.Fail(string.Join(" ", result.Errors.Select(error => error.Description)));
    }

    public async Task<bool> CheckPasswordAsync(User user, string password, CancellationToken ct) =>
        await _users.CheckPasswordAsync(await RequireAsync(user), password);

    public async Task<string> GenerateVerificationCodeAsync(User user, LoginChannel channel, CancellationToken ct) =>
        await _users.GenerateUserTokenAsync(await RequireAsync(user), ProviderFor(channel), VerificationPurpose);

    public async Task<bool> VerifyCodeAsync(User user, LoginChannel channel, string code, CancellationToken ct) =>
        await _users.VerifyUserTokenAsync(await RequireAsync(user), ProviderFor(channel), VerificationPurpose, code);

    public async Task<bool> IsLockedOutAsync(User user, CancellationToken ct) =>
        await _users.IsLockedOutAsync(await RequireAsync(user));

    public async Task RegisterFailedAttemptAsync(User user, CancellationToken ct) =>
        await _users.AccessFailedAsync(await RequireAsync(user));

    public async Task ResetFailedAttemptsAsync(User user, CancellationToken ct) =>
        await _users.ResetAccessFailedCountAsync(await RequireAsync(user));

    public async Task LockAsync(User user, DateTimeOffset? until, CancellationToken ct)
    {
        var entity = await RequireAsync(user);

        await _users.SetLockoutEnabledAsync(entity, true);
        await _users.SetLockoutEndDateAsync(entity, until ?? DateTimeOffset.MaxValue);
        await _users.UpdateSecurityStampAsync(entity);
    }

    public async Task UnlockAsync(User user, CancellationToken ct)
    {
        var entity = await RequireAsync(user);

        await _users.SetLockoutEndDateAsync(entity, null);
        await _users.ResetAccessFailedCountAsync(entity);
    }

    public async Task SetRolesAsync(User user, IReadOnlyList<string> roles, CancellationToken ct)
    {
        var entity = await RequireAsync(user);
        var current = await _users.GetRolesAsync(entity);

        await _users.RemoveFromRolesAsync(entity, current.Except(roles));
        await _users.AddToRolesAsync(entity, roles.Except(current));
    }

    private async Task<ApplicationUser> RequireAsync(User user) =>
        await _users.FindByIdAsync(user.Id.ToString())
        ?? throw new DomainException($"La cuenta '{user.Id}' ya no existe.");

    private static string ProviderFor(LoginChannel channel) =>
        channel == LoginChannel.Email ? TokenOptions.DefaultEmailProvider : TokenOptions.DefaultPhoneProvider;
}
