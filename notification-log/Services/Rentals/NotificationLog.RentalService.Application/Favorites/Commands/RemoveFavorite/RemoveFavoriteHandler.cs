using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Favorites;
using NotificationLog.RentalService.Domain.Favorites.ValueObjects;

namespace NotificationLog.RentalService.Application.Favorites.Commands.RemoveFavorite;

public sealed class RemoveFavoriteHandler
{
    private readonly IFavoriteListRepository _favorites;
    private readonly IUnitOfWork _uow;

    public RemoveFavoriteHandler(IFavoriteListRepository favorites, IUnitOfWork uow)
        => (_favorites, _uow) = (favorites, uow);

    public async Task HandleAsync(RemoveFavoriteCommand cmd, CancellationToken ct)
    {
        if (await _favorites.LoadAsync(FavoriteListId.For(cmd.UserId), ct) is not { } favorites)
            return;

        favorites.Remove(cmd.ListingId);

        await _favorites.AppendAsync(favorites, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
