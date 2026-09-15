using Marten;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Listings.Queries.ListListings;
using NotificationLog.RentalService.Application.SavedSearches.Queries.GetSavedSearches;
using NotificationLog.RentalService.Domain.Favorites.ValueObjects;
using NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

internal sealed class MartenUserCollectionsReadModel : IUserCollectionsReadModel
{
    private readonly IQuerySession _session;

    public MartenUserCollectionsReadModel(IQuerySession session) => _session = session;

    public async Task<IReadOnlyList<ListingSummaryDto>> GetFavoritesAsync(Guid userId, CancellationToken ct)
    {
        if (await _session.LoadAsync<FavoriteListDecision>(FavoriteListId.For(userId), ct) is not { ListingIds.Count: > 0 } favorites)
            return [];

        var listings = await _session.LoadManyAsync<ListingView>(ct, favorites.ListingIds);
        var byId = listings.ToDictionary(x => x.Id);

        return favorites.ListingIds
            .Where(byId.ContainsKey)
            .Select(id => MartenListingReadModel.ToSummary(byId[id]))
            .ToList();
    }

    public async Task<IReadOnlyList<SavedSearchDto>> GetSavedSearchesAsync(Guid userId, CancellationToken ct)
        => await _session.LoadAsync<SavedSearchListDecision>(SavedSearchListId.For(userId), ct) is { } list
            ? list.Searches.Select(s => new SavedSearchDto(
                    s.SearchId,
                    s.Name,
                    s.Frequency.ToString(),
                    s.Operation.ToString(),
                    s.City,
                    s.Neighborhoods,
                    s.MinPrice,
                    s.MaxPrice,
                    s.MinBedrooms,
                    s.MinStratum,
                    s.MaxStratum,
                    s.MinArea))
                .ToList()
            : [];
}
