using System.Security.Cryptography;
using Users.Application.Ports;
using Users.Domain;

namespace Users.Application;

public sealed record VerificationTokenResult(User User, string Token);

/// <summary>
/// Orchestration only: load, call the aggregate's Decide* method, apply the resulting events
/// locally, hand off to the repository port to publish + adopt as canonical state. No outbox and
/// no integration-event mapping here, unlike event-sourcing/'s UserCommandService — there is
/// nothing to translate at a boundary in this architecture, and nothing to dual-write, because
/// the log itself is the only durable store (event-driven/README.md).
/// </summary>
public sealed class UserCommandService(IUserRepository repository, TimeProvider clock)
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);

    public async Task<User> RegisterUserAsync(string name, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var now = clock.GetUtcNow();

        var registered = User.DecideRegisterUser(new RegisterUser(userId, name), now);
        var user = User.Create(registered);
        await repository.StartAsync(user, registered, ct);
        return user;
    }

    public async Task<VerificationTokenResult> RequestEmailChangeAsync(Guid userId, string email, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var now = clock.GetUtcNow();
        var token = GenerateToken();

        var command = new RequestEmailChange(userId, email, TokenHasher.Hash(token), now.Add(TokenLifetime));
        var events = user.DecideRequestEmailChange(command, now);

        await SaveAsync(user, events, ct);
        return new VerificationTokenResult(user, token);
    }

    public async Task<User> VerifyEmailAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideVerifyEmail(new VerifyEmail(userId, token), clock.GetUtcNow());
        await SaveAsync(user, events, ct);
        return user;
    }

    public async Task<VerificationTokenResult> RequestPhoneChangeAsync(Guid userId, string phone, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var now = clock.GetUtcNow();
        var token = GenerateToken();

        var command = new RequestPhoneChange(userId, phone, TokenHasher.Hash(token), now.Add(TokenLifetime));
        var events = user.DecideRequestPhoneChange(command, now);

        await SaveAsync(user, events, ct);
        return new VerificationTokenResult(user, token);
    }

    public async Task<User> VerifyPhoneAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideVerifyPhone(new VerifyPhone(userId, token), clock.GetUtcNow());
        await SaveAsync(user, events, ct);
        return user;
    }

    public async Task<User> ChangePreferencesAsync(Guid userId, NotificationPreferences preferences, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideChangePreferences(new ChangePreferences(userId, preferences), clock.GetUtcNow());
        await SaveAsync(user, events, ct);
        return user;
    }

    public async Task<User> DeactivateUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideDeactivateUser(new DeactivateUser(userId), clock.GetUtcNow());
        await SaveAsync(user, events, ct);
        return user;
    }

    public async Task<User> ReactivateUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideReactivateUser(new ReactivateUser(userId), clock.GetUtcNow());
        await SaveAsync(user, events, ct);
        return user;
    }

    private async Task<User> LoadOrThrowAsync(Guid userId, CancellationToken ct) =>
        await repository.LoadAsync(userId, ct) ?? throw new UserNotFoundException(userId);

    private async Task SaveAsync(User user, IReadOnlyList<object> newEvents, CancellationToken ct)
    {
        foreach (var e in newEvents)
            user.Apply(e);

        await repository.AppendAsync(user.Id, user, newEvents, ct);
    }

    private static string GenerateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
}
