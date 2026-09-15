using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Application.Listings.Commands.ExpireReservation;

public sealed class ExpireReservationHandler
{
    private readonly IListingRepository _listings;
    private readonly IOfferRepository _offers;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public ExpireReservationHandler(IListingRepository listings, IOfferRepository offers, IUnitOfWork uow, TimeProvider time)
        => (_listings, _offers, _uow, _time) = (listings, offers, uow, time);

    public async Task HandleAsync(ExpireReservationCommand cmd, CancellationToken ct)
    {
        if (await _listings.LoadAsync(cmd.ListingId, ct) is not { } listing)
            return;

        var offerId = listing.ReservedOfferId;

        listing.ExpireReservation(_time.GetUtcNow());

        if (listing.Status != ListingStatus.Reserved
            && offerId is { } id
            && await _offers.LoadAsync(id, ct) is { } offer)
        {
            offer.FallThrough("La reserva venció sin cierre.");
            await _offers.AppendAsync(offer, ct);
        }

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
