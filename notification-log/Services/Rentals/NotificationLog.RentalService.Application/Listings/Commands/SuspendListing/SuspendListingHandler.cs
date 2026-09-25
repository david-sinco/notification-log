using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;

public sealed class SuspendListingHandler(
    IListingRepository listings,
    IUnitOfWork uow,
    IValidator<SuspendListingCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<SuspendListingCommand> _validator = validator;

    public async Task HandleAsync(SuspendListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        listing.Suspend(cmd.Reason);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
