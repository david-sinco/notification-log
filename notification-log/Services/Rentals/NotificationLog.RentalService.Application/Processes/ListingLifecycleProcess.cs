using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Application.Listings.Commands.ExpireReservation;
using NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;
using NotificationLog.RentalService.Domain.Inquiries;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Offers;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ListingLifecycleProcess
{
    private const string ReservedReason = "El inmueble fue reservado con otra oferta.";
    private const string UnavailableReason = "La publicación ya no está disponible.";

    private readonly IOfferRepository _offers;
    private readonly IVisitRepository _visits;
    private readonly IInquiryRepository _inquiries;
    private readonly IProcessLookups _lookups;
    private readonly ICommandScheduler _scheduler;
    private readonly INotificationDispatcher _notifications;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public ListingLifecycleProcess(
        IOfferRepository offers,
        IVisitRepository visits,
        IInquiryRepository inquiries,
        IProcessLookups lookups,
        ICommandScheduler scheduler,
        INotificationDispatcher notifications,
        IUnitOfWork uow,
        TimeProvider time)
        => (_offers, _visits, _inquiries, _lookups, _scheduler, _notifications, _uow, _time)
            = (offers, visits, inquiries, lookups, scheduler, notifications, uow, time);

    public async Task OnPublishedAsync(Guid listingId, DateTimeOffset expiresAt, CancellationToken ct)
    {
        await _scheduler.ScheduleAsync(new WarnListingExpiryCommand(listingId, expiresAt), expiresAt - ListingPolicy.ExpiryNotice, ct);
        await _scheduler.ScheduleAsync(new ExpireListingCommand(listingId), expiresAt, ct);
    }

    public async Task OnReservedAsync(Guid listingId, Guid offerId, DateTimeOffset reservedUntil, CancellationToken ct)
    {
        await RejectOpenOffersAsync(listingId, offerId, ReservedReason, ct);
        await _uow.SaveChangesAsync(ct);

        await _scheduler.ScheduleAsync(new ExpireReservationCommand(listingId), reservedUntil, ct);
    }

    public Task OnReservationExtendedAsync(Guid listingId, DateTimeOffset reservedUntil, CancellationToken ct)
        => _scheduler.ScheduleAsync(new ExpireReservationCommand(listingId), reservedUntil, ct);

    public async Task OnNoLongerAvailableAsync(Guid listingId, CancellationToken ct)
    {
        var now = _time.GetUtcNow();

        await RejectOpenOffersAsync(listingId, null, UnavailableReason, ct);

        foreach (var visitId in await _lookups.FindUpcomingVisitIdsAsync(listingId, ct))
        {
            if (await _visits.LoadAsync(visitId, ct) is not { } visit
                || visit.Status is not (VisitStatus.Requested or VisitStatus.Confirmed)
                || (visit.ConfirmedSlot is { } slot && slot.Start <= now))
                continue;

            visit.Cancel(VisitParty.Host, UnavailableReason, now);
            await _visits.AppendAsync(visit, ct);
        }

        foreach (var inquiryId in await _lookups.FindOpenInquiryIdsAsync(listingId, ct))
        {
            if (await _inquiries.LoadAsync(inquiryId, ct) is not { IsClosed: false } inquiry)
                continue;

            inquiry.Close(UnavailableReason);
            await _inquiries.AppendAsync(inquiry, ct);
        }

        await _uow.SaveChangesAsync(ct);

        var data = new Dictionary<string, string> { ["listing_id"] = listingId.ToString() };

        foreach (var userId in await _lookups.FindFavoritedByAsync(listingId, ct))
            await _notifications.DispatchAsync(NotificationKeys.FavoriteUnavailable, userId, data, ct);
    }

    private async Task RejectOpenOffersAsync(Guid listingId, Guid? exceptOfferId, string reason, CancellationToken ct)
    {
        foreach (var offerId in await _lookups.FindOpenOfferIdsAsync(listingId, ct))
        {
            if (offerId == exceptOfferId || await _offers.LoadAsync(offerId, ct) is not { IsOpen: true } offer)
                continue;

            offer.Reject(reason);
            await _offers.AppendAsync(offer, ct);
        }
    }
}
