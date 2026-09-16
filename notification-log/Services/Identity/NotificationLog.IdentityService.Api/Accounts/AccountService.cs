using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NotificationLog.Contracts.Identity;
using NotificationLog.IdentityService.Api.Accounts.Verification;
using NotificationLog.IdentityService.Api.Data;
using Wolverine.EntityFrameworkCore;

namespace NotificationLog.IdentityService.Api.Accounts;

public sealed class AccountService
{
    private const string VerificationPurpose = "VerifyAccount";

    private readonly UserManager<ApplicationUser> _users;
    private readonly IDbContextOutbox<IdentityServiceDbContext> _outbox;
    private readonly VerificationCodeSender _codes;
    private readonly TimeProvider _time;

    public AccountService(
        UserManager<ApplicationUser> users,
        IDbContextOutbox<IdentityServiceDbContext> outbox,
        VerificationCodeSender codes,
        TimeProvider time)
        => (_users, _outbox, _codes, _time) = (users, outbox, codes, time);

    public async Task<AccountResult> RegisterAsync(string identifier, string password, CancellationToken ct)
    {
        var login = LoginIdentifier.TryParse(identifier);

        if (login is null)
            return AccountResult.Fail("Escribe un correo o un teléfono válido.");

        if (AccountPolicy.ValidatePassword(password) is { } passwordError)
            return AccountResult.Fail(passwordError);

        var user = await FindAsync(login, ct);

        if (user is { IsVerified: true })
            return AccountResult.Fail("Ya existe una cuenta con ese correo o teléfono.");

        if (user is null)
        {
            user = new ApplicationUser(Guid.NewGuid())
            {
                Email = login.Channel == LoginChannel.Email ? login.Value : null,
                PhoneNumber = login.Channel == LoginChannel.Phone ? login.Value : null
            };

            var created = await _users.CreateAsync(user, password);

            if (!created.Succeeded)
                return AccountResult.Fail(Describe(created));
        }
        else
        {
            await _users.RemovePasswordAsync(user);

            var replaced = await _users.AddPasswordAsync(user, password);

            if (!replaced.Succeeded)
                return AccountResult.Fail(Describe(replaced));
        }

        await SendCodeAsync(user, login, ct);

        return AccountResult.Ok();
    }

    public async Task ResendCodeAsync(string identifier, CancellationToken ct)
    {
        var login = LoginIdentifier.TryParse(identifier);
        var user = login is null ? null : await FindAsync(login, ct);

        if (login is null || user is null || user.IsConfirmed(login.Channel))
            return;

        await SendCodeAsync(user, login, ct);
    }

    public async Task<AccountResult> VerifyAsync(string identifier, string code, CancellationToken ct)
    {
        var invalid = AccountResult.Fail("El código no es válido o ya venció.");

        var login = LoginIdentifier.TryParse(identifier);
        var user = login is null ? null : await FindAsync(login, ct);

        if (login is null || user is null)
            return invalid;

        if (user.IsConfirmed(login.Channel))
            return AccountResult.Ok();

        if (await _users.IsLockedOutAsync(user))
            return AccountResult.Fail("Hiciste demasiados intentos. Intenta de nuevo más tarde.");

        if (!await _users.VerifyUserTokenAsync(user, ProviderFor(login.Channel), VerificationPurpose, code.Trim()))
        {
            await _users.AccessFailedAsync(user);
            return invalid;
        }

        var isNewAccount = !user.IsVerified;

        if (login.Channel == LoginChannel.Email)
            user.EmailConfirmed = true;
        else
            user.PhoneNumberConfirmed = true;

        user.AccessFailedCount = 0;

        if (isNewAccount)
        {
            await _outbox.PublishAsync(new UserCreated
            {
                EventId = Guid.NewGuid().ToString(),
                OccurredAt = Timestamp.FromDateTimeOffset(_time.GetUtcNow()),
                SchemaVersion = 1,
                UserId = user.Id.ToString(),
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber ?? string.Empty
            });
        }

        await _outbox.SaveChangesAndFlushMessagesAsync(ct);

        return AccountResult.Ok();
    }

    public async Task<SignInAttempt> SignInAsync(string identifier, string password, CancellationToken ct)
    {
        var login = LoginIdentifier.TryParse(identifier);
        var user = login is null ? null : await FindAsync(login, ct);

        if (login is null || user is null)
            return SignInAttempt.Failed(SignInStatus.InvalidCredentials);

        if (await _users.IsLockedOutAsync(user))
            return SignInAttempt.Failed(SignInStatus.LockedOut);

        if (!await _users.CheckPasswordAsync(user, password))
        {
            await _users.AccessFailedAsync(user);

            return SignInAttempt.Failed(await _users.IsLockedOutAsync(user)
                ? SignInStatus.LockedOut
                : SignInStatus.InvalidCredentials);
        }

        await _users.ResetAccessFailedCountAsync(user);

        if (!user.IsConfirmed(login.Channel))
            return SignInAttempt.Failed(SignInStatus.NotVerified);

        return new SignInAttempt(SignInStatus.Succeeded, await ToSignedInUserAsync(user));
    }

    public async Task<SignedInUser?> ValidateSessionAsync(Guid userId, string securityStamp, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId.ToString());

        if (user is null
            || !user.IsVerified
            || user.SecurityStamp != securityStamp
            || await _users.IsLockedOutAsync(user))
            return null;

        return await ToSignedInUserAsync(user);
    }

    private Task<ApplicationUser?> FindAsync(LoginIdentifier login, CancellationToken ct) =>
        login.Channel == LoginChannel.Email
            ? _users.FindByEmailAsync(login.Value)
            : _users.Users.SingleOrDefaultAsync(user => user.PhoneNumber == login.Value, ct);

    private async Task SendCodeAsync(ApplicationUser user, LoginIdentifier login, CancellationToken ct)
    {
        var code = await _users.GenerateUserTokenAsync(user, ProviderFor(login.Channel), VerificationPurpose);
        await _codes.SendAsync(login, code, ct);
    }

    private async Task<SignedInUser> ToSignedInUserAsync(ApplicationUser user) =>
        new(user.Id, user.Email, user.PhoneNumber, user.SecurityStamp!, [.. await _users.GetRolesAsync(user)]);

    private static string ProviderFor(LoginChannel channel) =>
        channel == LoginChannel.Email ? TokenOptions.DefaultEmailProvider : TokenOptions.DefaultPhoneProvider;

    private static string Describe(IdentityResult result) =>
        string.Join(" ", result.Errors.Select(error => error.Description));
}
