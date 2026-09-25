using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingPhotos;

public sealed class UpdateListingPhotosHandler(
    IListingRepository listings,
    IUnitOfWork uow,
    IValidator<UpdateListingPhotosCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<UpdateListingPhotosCommand> _validator = validator;

    public async Task HandleAsync(UpdateListingPhotosCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        Listing.EnsureCanManage(listing, user);

        listing.UpdatePhotos(cmd.Photos.Select(Photo.Create).ToList());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
