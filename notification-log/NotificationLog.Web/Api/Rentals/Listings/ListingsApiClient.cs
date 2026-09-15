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

    public Task SubmitForReviewAsync(Guid id, Guid actorId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/submit", new ActorRequest(actorId), ct);

    public Task SetAvailabilityAsync(Guid id, Guid actorId, bool isAvailable, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Patch, $"/api/listings/{id}/availability", new SetListingAvailabilityRequest(actorId, isAvailable), ct);

    public Task RenewAsync(Guid id, Guid actorId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/renew", new ActorRequest(actorId), ct);

    public Task AssignAdvisorAsync(Guid id, Guid actorId, Guid? advisorId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Put, $"/api/listings/{id}/advisor", new AssignAdvisorRequest(actorId, advisorId), ct);

    public Task WithdrawAsync(Guid id, Guid actorId, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/withdraw", new ReasonRequest(actorId, reason), ct);

    public Task ReportAsync(Guid id, Guid reporterId, ReportReason reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/reports", new ReportListingRequest(reporterId, reason), ct);

    public Task ExtendReservationAsync(Guid id, Guid actorId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/reservation/extend", new ActorRequest(actorId), ct);

    public Task CancelReservationAsync(Guid id, Guid actorId, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/reservation/cancel", new ReasonRequest(actorId, reason), ct);

    public Task CloseAsync(Guid id, CloseListingRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/listings/{id}/close", request, ct);
}
