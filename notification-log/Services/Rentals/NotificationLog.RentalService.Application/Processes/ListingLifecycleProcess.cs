using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ListingLifecycleProcess
{
    private const string UnavailableReason = "La publicación ya no está disponible.";

    private readonly IVisitRepository _visits;
    private readonly IProcessLookups _lookups;
    private readonly ICommandScheduler _scheduler;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public ListingLifecycleProcess(
        IVisitRepository visits,
        IProcessLookups lookups,
        ICommandScheduler scheduler,
        IUnitOfWork uow,
        TimeProvider time)
        => (_visits, _lookups, _scheduler, _uow, _time) = (visits, lookups, scheduler, uow, time);

    public async Task OnPublishedAsync(Guid listingId, DateTimeOffset expiresAt, CancellationToken ct)
    {
        await _scheduler.ScheduleAsync(new WarnListingExpiryCommand(listingId, expiresAt), expiresAt - ListingPolicy.ExpiryNotice, ct);
        await _scheduler.ScheduleAsync(new ExpireListingCommand(listingId), expiresAt, ct);
    }

    public async Task OnNoLongerAvailableAsync(Guid listingId, CancellationToken ct)
    {
        var now = _time.GetUtcNow();

        foreach (var visitId in await _lookups.FindUpcomingVisitIdsAsync(listingId, ct))
        {
            if (await _visits.LoadAsync(visitId, ct) is not { } visit
                || visit.Status is not (VisitStatus.Requested or VisitStatus.Confirmed)
                || (visit.ConfirmedSlot is { } slot && slot.Start <= now))
                continue;

            visit.Cancel(VisitParty.Host, UnavailableReason, now);
            await _visits.AppendAsync(visit, ct);
        }

        await _uow.SaveChangesAsync(ct);
    }
}
