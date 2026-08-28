using System.Security.Cryptography;
using Users.Application.IntegrationEvents;
using Users.Application.Ports;
using Users.Domain;

namespace Users.Application;

public sealed record VerificationTokenResult(User User, string Token);

/// <summary>
/// Orchestration only, per SPEC.md §3: load, call the aggregate's Decide* method, translate any
/// resulting integration event at the boundary, and hand off to the repository port to persist.
/// No business rules live here — those are entirely in Users.Domain.
/// </summary>
public sealed class UserCommandService(
    IUserRepository repository, IIntegrationEventOutbox outbox, TimeProvider clock)
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromMinutes(15);

    public async Task<User> RegisterUserAsync(string name, CancellationToken ct)
    {
        var userId = Guid.NewGuid();
        var now = clock.GetUtcNow();

        var registered = User.DecideRegisterUser(new RegisterUser(userId, name), now);
        await repository.StartAsync(userId, registered, ct);

        var user = new User();
        user.Apply(registered);
        return user;
    }

    public async Task<VerificationTokenResult> RequestEmailChangeAsync(Guid userId, string email, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var now = clock.GetUtcNow();
        var token = GenerateToken();

        var command = new RequestEmailChange(userId, email, TokenHasher.Hash(token), now.Add(TokenLifetime));
        var events = user.DecideRequestEmailChange(command, now);

        await SaveAsync(user, events, reservationChange: null, ct);
        return new VerificationTokenResult(user, token);
    }

    public async Task<User> VerifyEmailAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var previouslyVerified = user.VerifiedEmail;

        var events = user.DecideVerifyEmail(new VerifyEmail(userId, token), clock.GetUtcNow());
        var verified = events.OfType<EmailVerified>().FirstOrDefault();
        var reservationChange = verified is null
            ? null
            : new EmailReservationChange(Release: previouslyVerified, Reserve: verified.Email);

        await SaveAsync(user, events, reservationChange, ct);
        return user;
    }

    public async Task<VerificationTokenResult> RequestPhoneChangeAsync(Guid userId, string phone, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var now = clock.GetUtcNow();
        var token = GenerateToken();

        var command = new RequestPhoneChange(userId, phone, TokenHasher.Hash(token), now.Add(TokenLifetime));
        var events = user.DecideRequestPhoneChange(command, now);

        await SaveAsync(user, events, reservationChange: null, ct);
        return new VerificationTokenResult(user, token);
    }

    public async Task<User> VerifyPhoneAsync(Guid userId, string token, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideVerifyPhone(new VerifyPhone(userId, token), clock.GetUtcNow());
        await SaveAsync(user, events, reservationChange: null, ct);
        return user;
    }

    public async Task<User> ChangePreferencesAsync(Guid userId, NotificationPreferences preferences, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideChangePreferences(new ChangePreferences(userId, preferences), clock.GetUtcNow());
        await SaveAsync(user, events, reservationChange: null, ct);
        return user;
    }

    public async Task<User> DeactivateUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideDeactivateUser(new DeactivateUser(userId), clock.GetUtcNow());
        await SaveAsync(user, events, reservationChange: null, ct);
        return user;
    }

    public async Task<User> ReactivateUserAsync(Guid userId, CancellationToken ct)
    {
        var user = await LoadOrThrowAsync(userId, ct);
        var events = user.DecideReactivateUser(new ReactivateUser(userId), clock.GetUtcNow());
        await SaveAsync(user, events, reservationChange: null, ct);
        return user;
    }

    private async Task<User> LoadOrThrowAsync(Guid userId, CancellationToken ct) =>
        await repository.LoadAsync(userId, ct) ?? throw new UserNotFoundException(userId);

    private async Task SaveAsync(
        User user, IReadOnlyList<object> newEvents, EmailReservationChange? reservationChange, CancellationToken ct)
    {
        foreach (var e in newEvents)
            user.Apply(e);

        var integrationEvent = IntegrationEventMapper.MapIfChanged(user, newEvents);
        if (integrationEvent is not null)
        {
            var envelope = new Envelope<UserContactUpdated>(
                EventId: Guid.NewGuid(),
                Type: "UserContactUpdated",
                AggregateId: user.Id,
                Version: user.Version,
                OccurredAt: clock.GetUtcNow(),
                SchemaVersion: 1,
                Data: integrationEvent);

            outbox.Enqueue(IntegrationTopics.UsersContact, user.Id.ToString(), envelope);
        }

        await repository.AppendAsync(user.Id, newEvents, reservationChange, ct);
    }

    private static string GenerateToken() => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
}
