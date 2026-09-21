using Google.Protobuf.WellKnownTypes;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Abstractions;
using Wolverine;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Publishers;

internal sealed class WolverineAccountProvisioner : IAccountProvisioner
{
    private readonly IMessageBus _bus;
    private readonly TimeProvider _time;

    public WolverineAccountProvisioner(IMessageBus bus, TimeProvider time) => (_bus, _time) = (bus, time);

    public async Task RequestAccountAsync(AccountRequest request, CancellationToken ct)
    {
        var message = new AccountCreationRequested
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTimeOffset(_time.GetUtcNow()),
            SchemaVersion = 1,
            UserId = request.UserId.ToString(),
            Identifier = request.Email,
            Phone = request.Phone,
            Name = request.Name,
            Locale = string.Empty,
            TimeZone = string.Empty,
            AcceptsNotifications = true
        };

        message.Roles.AddRange(request.Roles.Select(role => role.ToString()));

        await _bus.PublishAsync(message);
    }
}
