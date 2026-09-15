using NotificationLog.RentalService.Domain.Favorites;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenFavoriteListRepository : IFavoriteListRepository
{
    private readonly AggregateStreams _streams;

    public MartenFavoriteListRepository(AggregateStreams streams) => _streams = streams;

    public Task<FavoriteList?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<FavoriteList>(id, cancellationToken);

    public Task AppendAsync(FavoriteList favoriteList, CancellationToken cancellationToken = default)
    {
        _streams.Append(favoriteList);
        return Task.CompletedTask;
    }
}
