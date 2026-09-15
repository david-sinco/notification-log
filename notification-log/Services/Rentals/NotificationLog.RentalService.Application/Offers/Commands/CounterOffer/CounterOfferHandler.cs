using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Offers;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Offers.Commands.CounterOffer;

public sealed class CounterOfferHandler
{
    private readonly IOfferRepository _offers;
    private readonly IListingRepository _listings;
    private readonly OfferAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CounterOfferCommand> _validator;
    private readonly TimeProvider _time;

    public CounterOfferHandler(
        IOfferRepository offers,
        IListingRepository listings,
        OfferAccess access,
        IUnitOfWork uow,
        IValidator<CounterOfferCommand> validator,
        TimeProvider time)
        => (_offers, _listings, _access, _uow, _validator, _time) = (offers, listings, access, uow, validator, time);

    public async Task HandleAsync(CounterOfferCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var offer = await _offers.GetAsync(cmd.OfferId, ct);
        var listing = await _listings.GetAsync(offer.ListingId, ct);
        var party = await _access.ResolvePartyAsync(offer, listing, cmd.ActorId, ct);

        offer.Counter(party, Money.Create(cmd.Amount), _time.GetUtcNow());

        await _offers.AppendAsync(offer, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
