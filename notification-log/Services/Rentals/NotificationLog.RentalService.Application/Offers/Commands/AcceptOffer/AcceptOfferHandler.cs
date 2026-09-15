using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Application.Offers.Commands.AcceptOffer;

public sealed class AcceptOfferHandler
{
    private readonly IOfferRepository _offers;
    private readonly IListingRepository _listings;
    private readonly OfferAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public AcceptOfferHandler(
        IOfferRepository offers, IListingRepository listings, OfferAccess access, IUnitOfWork uow, TimeProvider time)
        => (_offers, _listings, _access, _uow, _time) = (offers, listings, access, uow, time);

    public async Task HandleAsync(AcceptOfferCommand cmd, CancellationToken ct)
    {
        var offer = await _offers.GetAsync(cmd.OfferId, ct);
        var listing = await _listings.GetAsync(offer.ListingId, ct);
        var party = await _access.ResolvePartyAsync(offer, listing, cmd.ActorId, ct);
        var now = _time.GetUtcNow();

        offer.Accept(party, now);
        listing.Reserve(offer.Id, now);

        await _offers.AppendAsync(offer, ct);
        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
