using NotificationLog.RentalService.Application.Abstractions;
using Wolverine;

namespace NotificationLog.RentalService.Infrastructure.Scheduling;

internal sealed class WolverineCommandScheduler : ICommandScheduler
{
    private readonly IMessageBus _bus;

    public WolverineCommandScheduler(IMessageBus bus) => _bus = bus;

    public async Task ScheduleAsync(object command, DateTimeOffset at, CancellationToken ct)
        => await _bus.ScheduleAsync(command, at);
}
