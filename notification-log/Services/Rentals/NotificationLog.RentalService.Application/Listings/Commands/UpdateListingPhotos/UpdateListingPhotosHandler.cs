using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingPhotos;

public sealed class UpdateListingPhotosHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateListingPhotosCommand> _validator;

    public UpdateListingPhotosHandler(
        IListingRepository listings, ListingAccess access, IUnitOfWork uow, IValidator<UpdateListingPhotosCommand> validator)
        => (_listings, _access, _uow, _validator) = (listings, access, uow, validator);

    public async Task HandleAsync(UpdateListingPhotosCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        listing.UpdatePhotos(cmd.Photos.Select(Photo.Create).ToList());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
