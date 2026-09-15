using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Favorites;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Favorites.ValueObjects;

namespace NotificationLog.RentalService.Application.Favorites.Commands.AddFavorite;

public sealed class AddFavoriteHandler
{
    private readonly IListingRepository _listings;
    private readonly IFavoriteListRepository _favorites;
    private readonly IUnitOfWork _uow;

    public AddFavoriteHandler(IListingRepository listings, IFavoriteListRepository favorites, IUnitOfWork uow)
        => (_listings, _favorites, _uow) = (listings, favorites, uow);

    public async Task HandleAsync(AddFavoriteCommand cmd, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        if (listing.Status != ListingStatus.Published)
            throw new AppValidationException("Solo se pueden guardar publicaciones publicadas.");

        var favorites = await _favorites.LoadAsync(FavoriteListId.For(cmd.UserId), ct) ?? FavoriteList.StartFor(cmd.UserId);
        favorites.Add(listing.Id);

        await _favorites.AppendAsync(favorites, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
