using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ListingLifecycleProcess
{
    private readonly ICommandScheduler _scheduler;

    public ListingLifecycleProcess(ICommandScheduler scheduler) => _scheduler = scheduler;

    public async Task OnPublishedAsync(Guid listingId, DateTimeOffset expiresAt, CancellationToken ct)
    {
        await _scheduler.ScheduleAsync(new WarnListingExpiryCommand(listingId, expiresAt), expiresAt - ListingPolicy.ExpiryNotice, ct);
        await _scheduler.ScheduleAsync(new ExpireListingCommand(listingId), expiresAt, ct);
    }
}
