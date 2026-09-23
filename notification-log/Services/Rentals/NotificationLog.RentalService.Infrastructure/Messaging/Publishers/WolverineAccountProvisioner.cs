using Google.Protobuf.WellKnownTypes;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Owners.Producers;
using Wolverine;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Publishers;

internal sealed class WolverineAccountProvisioner : IAccountProvisioner
{
    private readonly IMessageBus _bus;
    private readonly TimeProvider _time;

    public WolverineAccountProvisioner(IMessageBus bus, TimeProvider time) => (_bus, _time) = (bus, time);

    public async Task RequestAccountAsync(AccountCreationRequested request, CancellationToken ct)
    {
        request.EventId = Guid.NewGuid().ToString();
        request.OccurredAt = Timestamp.FromDateTimeOffset(_time.GetUtcNow());
        request.SchemaVersion = 1;

        await _bus.PublishAsync(request);
    }
}
