using NotificationLog.Web.Api.Rentals.Listings;

namespace NotificationLog.Web.Api.Rentals.Users;

public sealed class UserCollectionsApiClient(HttpClient http)
{
    public Task<IReadOnlyList<ListingSummaryDto>> GetFavoritesAsync(Guid userId, CancellationToken ct)
        => http.GetJsonAsync<IReadOnlyList<ListingSummaryDto>>($"/api/users/{userId}/favorites", ct);

    public Task AddFavoriteAsync(Guid userId, Guid listingId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Put, $"/api/users/{userId}/favorites/{listingId}", null, ct);

    public Task RemoveFavoriteAsync(Guid userId, Guid listingId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Delete, $"/api/users/{userId}/favorites/{listingId}", null, ct);

    public Task<IReadOnlyList<SavedSearchDto>> GetSavedSearchesAsync(Guid userId, CancellationToken ct)
        => http.GetJsonAsync<IReadOnlyList<SavedSearchDto>>($"/api/users/{userId}/saved-searches", ct);

    public Task<Guid> CreateSavedSearchAsync(Guid userId, SavedSearchRequest request, CancellationToken ct)
        => http.SendForIdAsync(HttpMethod.Post, $"/api/users/{userId}/saved-searches", request, ct);

    public Task UpdateSavedSearchAsync(Guid userId, Guid searchId, SavedSearchRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Put, $"/api/users/{userId}/saved-searches/{searchId}", request, ct);

    public Task DeleteSavedSearchAsync(Guid userId, Guid searchId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Delete, $"/api/users/{userId}/saved-searches/{searchId}", null, ct);
}
