using Application.Shared.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Offers;
using NotificationLog.RentalService.Domain.Offers.Enums;

namespace NotificationLog.RentalService.Application.Common;

public sealed class OfferAccess
{
    private readonly ListingAccess _listingAccess;

    public OfferAccess(ListingAccess listingAccess) => _listingAccess = listingAccess;

    public async Task<OfferParty> ResolvePartyAsync(Offer offer, Listing listing, Guid actorId, CancellationToken ct)
    {
        if (offer.OffererId == actorId)
            return OfferParty.Offerer;

        if (await _listingAccess.CanManageAsync(listing, actorId, ct))
            return OfferParty.Publisher;

        throw new AppValidationException("No participas en esta oferta.");
    }
}
