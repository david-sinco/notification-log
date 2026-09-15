using NotificationLog.RentalService.Application.Listings.Queries.ListListings;
using NotificationLog.RentalService.Application.SavedSearches.Queries.GetSavedSearches;

namespace NotificationLog.RentalService.Application.Abstractions;

public interface IUserCollectionsReadModel
{
    Task<IReadOnlyList<ListingSummaryDto>> GetFavoritesAsync(Guid userId, CancellationToken ct);

    Task<IReadOnlyList<SavedSearchDto>> GetSavedSearchesAsync(Guid userId, CancellationToken ct);
}
