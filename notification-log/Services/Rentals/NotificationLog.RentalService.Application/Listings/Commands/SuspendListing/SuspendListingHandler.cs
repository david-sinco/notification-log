using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;

public sealed class SuspendListingHandler
{
    private readonly IListingRepository _listings;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<SuspendListingCommand> _validator;

    public SuspendListingHandler(IListingRepository listings, IUnitOfWork uow, IValidator<SuspendListingCommand> validator)
        => (_listings, _uow, _validator) = (listings, uow, validator);

    public async Task HandleAsync(SuspendListingCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        listing.Suspend(cmd.Reason);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
