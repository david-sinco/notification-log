using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Offers;
using NotificationLog.RentalService.Domain.Offers.Enums;

namespace NotificationLog.RentalService.Application.Offers.Commands.RejectOffer;

public sealed class RejectOfferHandler
{
    private readonly IOfferRepository _offers;
    private readonly IListingRepository _listings;
    private readonly OfferAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RejectOfferCommand> _validator;

    public RejectOfferHandler(
        IOfferRepository offers,
        IListingRepository listings,
        OfferAccess access,
        IUnitOfWork uow,
        IValidator<RejectOfferCommand> validator)
        => (_offers, _listings, _access, _uow, _validator) = (offers, listings, access, uow, validator);

    public async Task HandleAsync(RejectOfferCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var offer = await _offers.GetAsync(cmd.OfferId, ct);
        var listing = await _listings.GetAsync(offer.ListingId, ct);

        if (await _access.ResolvePartyAsync(offer, listing, cmd.ActorId, ct) != OfferParty.Publisher)
            throw new AppValidationException("Solo el publicador rechaza ofertas; el interesado puede retirar la suya.");

        offer.Reject(cmd.Reason);

        await _offers.AppendAsync(offer, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
