namespace NotificationLog.Web.Api.Rentals.Offers;

public sealed class OffersApiClient(HttpClient http)
{
    public Task<PagedResult<OfferSummaryDto>> ListAsync(
        Guid? listingId, Guid? offererId, OfferStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("listingId", listingId?.ToString()),
            ("offererId", offererId?.ToString()),
            ("status", status?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<OfferSummaryDto>>($"/api/offers{query}", ct);
    }

    public Task<OfferDto> GetByIdAsync(Guid id, CancellationToken ct)
        => http.GetJsonAsync<OfferDto>($"/api/offers/{id}", ct);

    public Task<Guid> SubmitAsync(SubmitOfferRequest request, CancellationToken ct)
        => http.SendForIdAsync(HttpMethod.Post, "/api/offers", request, ct);

    public Task CounterAsync(Guid id, Guid actorId, long amount, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/offers/{id}/counter", new CounterOfferRequest(actorId, amount), ct);

    public Task AcceptAsync(Guid id, Guid actorId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/offers/{id}/accept", new OfferActorRequest(actorId), ct);

    public Task RejectAsync(Guid id, Guid actorId, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/offers/{id}/reject", new RejectOfferRequest(actorId, reason), ct);

    public Task WithdrawAsync(Guid id, Guid actorId, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/offers/{id}/withdraw", new OfferActorRequest(actorId), ct);
}
