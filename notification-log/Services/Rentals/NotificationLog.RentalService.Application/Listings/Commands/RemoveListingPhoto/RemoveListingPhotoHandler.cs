using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Application.Common.Producers;

namespace NotificationLog.RentalService.Application.Listings.Commands.RemoveListingPhoto;

public sealed class RemoveListingPhotoHandler(
    IListingRepository listings,
    IOwnerRepository owners,
    IUnitOfWork uow,
    INotificationProducer notifications,
    IPhotoStorage storage,
    IValidator<RemoveListingPhotoCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly IPhotoStorage _storage = storage;
    private readonly IValidator<RemoveListingPhotoCommand> _validator = validator;

    public async Task HandleAsync(RemoveListingPhotoCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        var owner = await _owners.GetAsync(listing.OwnerId, ct);
        listing.EnsureCanManage(user, owner.RelatedUserId);

        listing.RemovePhoto(cmd.FileName);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        await _storage.DeleteAsync(listing.Id, cmd.FileName, ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.PhotosUpdated,
            owner.HostUserId,
            new Dictionary<string, string>(ListingNotificationData.For(listing))
            {
                ["photo_count"] = listing.Photos.Count.ToString()
            },
            ct);
    }
}
