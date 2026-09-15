using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Application.Listings.Commands.CancelReservation;

public sealed class CancelReservationHandler
{
    private readonly IListingRepository _listings;
    private readonly IOfferRepository _offers;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CancelReservationCommand> _validator;
    private readonly TimeProvider _time;

    public CancelReservationHandler(
        IListingRepository listings,
        IOfferRepository offers,
        ListingAccess access,
        IUnitOfWork uow,
        IValidator<CancelReservationCommand> validator,
        TimeProvider time)
        => (_listings, _offers, _access, _uow, _validator, _time) = (listings, offers, access, uow, validator, time);

    public async Task HandleAsync(CancelReservationCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        var offerId = listing.ReservedOfferId
            ?? throw new AppValidationException("La publicación no está reservada.");

        var offer = await _offers.GetAsync(offerId, ct);

        if (offer.OffererId != cmd.ActorId)
            await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        listing.CancelReservation(cmd.Reason, _time.GetUtcNow());
        offer.FallThrough(cmd.Reason);

        await _listings.AppendAsync(listing, ct);
        await _offers.AppendAsync(offer, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
