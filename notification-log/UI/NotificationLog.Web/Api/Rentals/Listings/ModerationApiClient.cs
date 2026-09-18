namespace NotificationLog.Web.Api.Rentals.Listings;

public sealed class ModerationApiClient(HttpClient http)
{
    public Task ReviewAsync(Guid id, bool approve, IReadOnlyList<RejectionReason> reasons, CancellationToken ct)
        => http.SendJsonAsync(
            HttpMethod.Post, $"/api/moderation/listings/{id}/review", new ReviewListingRequest(approve, reasons), ct);

    public Task SuspendAsync(Guid id, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/moderation/listings/{id}/suspend", new ReasonRequest(reason), ct);

    public Task ReinstateAsync(Guid id, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/moderation/listings/{id}/reinstate", null, ct);

    public Task WithdrawAsync(Guid id, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/moderation/listings/{id}/withdraw", new ReasonRequest(reason), ct);
}
