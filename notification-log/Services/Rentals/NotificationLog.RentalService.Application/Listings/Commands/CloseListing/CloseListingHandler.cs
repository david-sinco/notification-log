using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Listings.Commands.CloseListing;

public sealed class CloseListingHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CloseListingCommand> _validator;
    private readonly TimeProvider _time;

    public CloseListingHandler(
        IListingRepository listings,
        ListingAccess access,
        IUnitOfWork uow,
        IValidator<CloseListingCommand> validator,
        TimeProvider time)
        => (_listings, _access, _uow, _validator, _time) = (listings, access, uow, validator, time);

    public async Task HandleAsync(CloseListingCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        var offerId = listing.ReservedOfferId
            ?? throw new AppValidationException("Solo se puede cerrar una publicación reservada.");

        listing.Close(offerId, Money.Create(cmd.FinalPrice), cmd.SignedOn, _time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
