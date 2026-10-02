using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.RemoveListingPhoto;

public sealed class RemoveListingPhotoHandler(
    IListingRepository listings,
    IUnitOfWork uow,
    IPhotoStorage storage,
    IValidator<RemoveListingPhotoCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly IPhotoStorage _storage = storage;
    private readonly IValidator<RemoveListingPhotoCommand> _validator = validator;

    public async Task HandleAsync(RemoveListingPhotoCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        Listing.EnsureCanManage(listing, user);

        listing.RemovePhoto(cmd.FileName);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        await _storage.DeleteAsync(listing.Id, cmd.FileName, ct);
    }
}
