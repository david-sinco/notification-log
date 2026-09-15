namespace NotificationLog.RentalService.Domain.Favorites;

public interface IFavoriteListRepository
{
    Task<FavoriteList?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(FavoriteList favoriteList, CancellationToken cancellationToken = default);
}
