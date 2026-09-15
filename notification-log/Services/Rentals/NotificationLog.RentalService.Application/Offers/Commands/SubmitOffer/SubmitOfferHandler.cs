using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Offers;
using NotificationLog.RentalService.Domain.Offers.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Offers.Commands.SubmitOffer;

public sealed class SubmitOfferHandler
{
    private readonly IListingRepository _listings;
    private readonly IOfferRepository _offers;
    private readonly ListingAccess _access;
    private readonly ISoftRuleChecks _rules;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<SubmitOfferCommand> _validator;
    private readonly TimeProvider _time;

    public SubmitOfferHandler(
        IListingRepository listings,
        IOfferRepository offers,
        ListingAccess access,
        ISoftRuleChecks rules,
        IUnitOfWork uow,
        IValidator<SubmitOfferCommand> validator,
        TimeProvider time)
        => (_listings, _offers, _access, _rules, _uow, _validator, _time) = (listings, offers, access, rules, uow, validator, time);

    public async Task<Guid> HandleAsync(SubmitOfferCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        if (listing.Status != ListingStatus.Published || listing.Price is not { } listedPrice)
            throw new AppValidationException("La publicación no está disponible para recibir ofertas.");

        if (await _access.CanManageAsync(listing, cmd.OffererId, ct))
            throw new AppValidationException("No puedes ofertar por tu propia publicación.");

        await _access.RequireVerifiedUserAsync(cmd.OffererId, requireDocument: true, ct);

        if (!await _rules.HasCompletedVisitAsync(cmd.OffererId, listing.Id, ct))
            throw new AppValidationException("Necesitas haber visitado el inmueble para hacer una oferta.");

        if (await _rules.HasOpenOfferAsync(cmd.OffererId, listing.Id, ct))
            throw new AppValidationException("Ya tienes una oferta abierta por esta publicación.");

        var now = _time.GetUtcNow();

        var terms = cmd.RentStartDate is { } start && cmd.RentTermMonths is { } months
            ? RentTerms.Create(start, months, ColombiaTime.Today(now))
            : null;

        var offer = Offer.Submit(
            Guid.NewGuid(), listing.Id, cmd.OffererId, listing.Operation, listedPrice, Money.Create(cmd.Amount), terms, now);

        await _offers.AppendAsync(offer, ct);
        await _uow.SaveChangesAsync(ct);

        return offer.Id;
    }
}
