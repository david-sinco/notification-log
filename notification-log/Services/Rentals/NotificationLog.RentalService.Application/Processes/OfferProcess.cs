using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Offers.Commands.ExpireOffer;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class OfferProcess
{
    private readonly ICommandScheduler _scheduler;

    public OfferProcess(ICommandScheduler scheduler) => _scheduler = scheduler;

    public Task OnAwaitingResponseAsync(Guid offerId, DateTimeOffset respondBy, CancellationToken ct)
        => _scheduler.ScheduleAsync(new ExpireOfferCommand(offerId), respondBy, ct);
}
