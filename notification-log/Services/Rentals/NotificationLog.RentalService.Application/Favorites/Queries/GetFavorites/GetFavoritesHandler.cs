using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Listings.Queries.ListListings;

namespace NotificationLog.RentalService.Application.Favorites.Queries.GetFavorites;

public sealed class GetFavoritesHandler
{
    private readonly IUserCollectionsReadModel _collections;

    public GetFavoritesHandler(IUserCollectionsReadModel collections) => _collections = collections;

    public Task<IReadOnlyList<ListingSummaryDto>> HandleAsync(GetFavoritesQuery query, CancellationToken ct)
        => _collections.GetFavoritesAsync(query.UserId, ct);
}
