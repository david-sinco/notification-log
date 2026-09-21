using Google.Protobuf.WellKnownTypes;
using NotificationLog.Contracts.Identity;
using NotificationLog.IdentityService.Application.Abstractions;
using NotificationLog.IdentityService.Domain.Users;
using NotificationLog.IdentityService.Domain.Users.Enums;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Infrastructure.Messaging;

internal sealed class WolverineUserEventPublisher : IUserEventPublisher
{
    private readonly IdentityOutbox _outbox;
    private readonly TimeProvider _time;

    public WolverineUserEventPublisher(IdentityOutbox outbox, TimeProvider time)
        => (_outbox, _time) = (outbox, time);

    public async Task PublishUserCreatedAsync(User user, CancellationToken ct) =>
        await _outbox.PublishAsync(new UserCreated
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTimeOffset(_time.GetUtcNow()),
            SchemaVersion = 1,
            UserId = user.Id.ToString(),
            Name = user.Name,
            Email = user.VerifiedEmail,
            Phone = user.VerifiedPhone,
            Locale = user.Locale,
            TimeZone = user.TimeZone,
            AcceptsNotifications = user.AcceptsNotifications
        });

    public async Task PublishVerificationChangedAsync(User user, CancellationToken ct) =>
        await _outbox.PublishAsync(new PersonVerificationChanged
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTimeOffset(_time.GetUtcNow()),
            SchemaVersion = 1,
            UserId = user.Id.ToString(),
            Email = user.VerifiedEmail,
            Phone = user.VerifiedPhone
        });

    public async Task PublishVerificationCodeRequestedAsync(LoginIdentifier login, string code, CancellationToken ct) =>
        await _outbox.PublishAsync(new VerificationCodeRequested
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTimeOffset(_time.GetUtcNow()),
            SchemaVersion = 1,
            Channel = login.Channel == LoginChannel.Email
                ? VerificationChannel.Email
                : VerificationChannel.Sms,
            Address = login.Value,
            Code = code
        });
}
