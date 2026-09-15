using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Listings.Events;
using NotificationLog.RentalService.Domain.Offers.Events;
using NotificationLog.RentalService.Domain.Visits.Events;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ProcessRouter
{
    private readonly ListingLifecycleProcess _lifecycle;
    private readonly OfferProcess _offers;
    private readonly VisitProcess _visits;
    private readonly PriceDropProcess _priceDrops;
    private readonly SavedSearchMatchingProcess _matching;
    private readonly ListingNotificationsProcess _notifications;

    public ProcessRouter(
        ListingLifecycleProcess lifecycle,
        OfferProcess offers,
        VisitProcess visits,
        PriceDropProcess priceDrops,
        SavedSearchMatchingProcess matching,
        ListingNotificationsProcess notifications)
        => (_lifecycle, _offers, _visits, _priceDrops, _matching, _notifications)
            = (lifecycle, offers, visits, priceDrops, matching, notifications);

    public async Task RouteAsync(Guid streamId, IDomainEvent domainEvent, CancellationToken ct)
    {
        switch (domainEvent)
        {
            case ListingApproved e:
                await _lifecycle.OnPublishedAsync(streamId, e.ExpiresAt, ct);
                await _matching.NotifyImmediateMatchesAsync(streamId, ct);
                break;

            case ListingRenewed e:
                await _lifecycle.OnPublishedAsync(streamId, e.ExpiresAt, ct);
                break;

            case ListingReserved e:
                await _lifecycle.OnReservedAsync(streamId, e.OfferId, e.ReservedUntil, ct);
                break;

            case ReservationExtended e:
                await _lifecycle.OnReservationExtendedAsync(streamId, e.ReservedUntil, ct);
                break;

            case ListingClosed or ListingWithdrawn:
                await _lifecycle.OnNoLongerAvailableAsync(streamId, ct);
                break;

            case ListingPriceChanged e:
                await _priceDrops.OnPriceChangedAsync(streamId, e.OldPrice, e.NewPrice, ct);
                break;

            case OfferSubmitted e:
                await _offers.OnAwaitingResponseAsync(streamId, e.RespondBy, ct);
                break;

            case OfferCountered e:
                await _offers.OnAwaitingResponseAsync(streamId, e.RespondBy, ct);
                break;

            case VisitRequested e:
                await _visits.OnRequestedAsync(streamId, e.RespondBy, ct);
                break;

            case VisitConfirmed e:
                await _visits.OnConfirmedAsync(streamId, e.SlotStart, ct);
                break;
        }

        await _notifications.NotifyAsync(streamId, domainEvent, ct);
    }
}
