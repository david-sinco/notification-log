namespace NotificationLog.Web.Api.Rentals.Listings;

public sealed class ListingsApiClient(HttpClient http)
{
    public Task<PagedResult<ListingSummaryDto>> ListAsync(
        string? search, ListingStatus? status, Operation? operation, Guid? participantId, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("search", search),
            ("status", status?.ToString()),
            ("operation", operation?.ToString()),
            ("participantId", participantId?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<ListingSummaryDto>>($"/api/listings{query}", ct);
    }

    public Task<ListingDto> GetByIdAsync(Guid id, CancellationToken ct)
        => http.GetJsonAsync<ListingDto>($"/api/listings/{id}", ct);

    public Task<Guid> DraftAsync(DraftListingRequest request, CancellationToken ct)
        => http.SendForIdAsync(HttpMethod.Post, "/api/listings", request, ct);

    public Task UpdateDetailsAsync(Guid id, UpdateListingDetailsRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Put, $"/api/listings/{id}/details", request, ct);

    public Task UpdatePhotosAsync(Guid id, UpdateListingPhotosRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Put, $"/api/listings/{id}/photos", request, ct);

    public Task ChangePriceAsync(Guid id, ChangeListingPriceRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Patch, $"/api/listings/{id}/price", request, ct);

    public Task SubmitForReviewAsync(Guid id, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/submit", null, ct);

    public Task SetAvailabilityAsync(Guid id, bool isAvailable, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Patch, $"/api/listings/{id}/availability", new SetListingAvailabilityRequest(isAvailable), ct);

    public Task RenewAsync(Guid id, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/renew", null, ct);

    public Task WithdrawAsync(Guid id, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/withdraw", new ReasonRequest(reason), ct);

    public Task CloseAsync(Guid id, CloseListingRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/close", request, ct);
}
