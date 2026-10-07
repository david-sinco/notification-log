using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.Application.Listings.Commands.AddListingPhoto;

public sealed class AddListingPhotoHandler(
    IListingRepository listings,
    IOwnerRepository owners,
    IUnitOfWork uow,
    IPhotoStorage storage)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly IPhotoStorage _storage = storage;

    public async Task<string> HandleAsync(AddListingPhotoCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        var owner = await _owners.GetAsync(listing.OwnerId, ct);
        listing.EnsureCanManage(user, owner.RelatedUserId);

        var photo = await Photo.FromStreamAsync(cmd.Content, ct);
        listing.AddPhoto(photo);

        await _storage.SaveAsync(listing.Id, photo.FileName, cmd.Content, photo.ContentType, ct);

        try
        {
            await _listings.AppendAsync(listing, ct);
            await _uow.SaveChangesAsync(ct);
        }
        catch
        {
            await _storage.DeleteAsync(listing.Id, photo.FileName, CancellationToken.None);
            throw;
        }

        return photo.FileName;
    }
}
