namespace NotificationLog.Web.Api.Rentals.Listings;

public sealed class ModerationApiClient(HttpClient http)
{
    public Task ReviewAsync(Guid id, Guid moderatorId, bool approve, IReadOnlyList<RejectionReason> reasons, CancellationToken ct)
        => http.SendJsonAsync(
            HttpMethod.Post, $"/api/moderation/listings/{id}/review", new ReviewListingRequest(moderatorId, approve, reasons), ct);

    public Task SuspendAsync(Guid id, Guid moderatorId, string reason, CancellationToken ct)
        => http.SendJsonAsync(
            HttpMethod.Post, $"/api/moderation/listings/{id}/suspend", new ModeratorReasonRequest(moderatorId, reason), ct);

    public Task ReinstateAsync(Guid id, Guid moderatorId, CancellationToken ct)
        => http.SendJsonAsync(
            HttpMethod.Post, $"/api/moderation/listings/{id}/reinstate", new ModeratorRequest(moderatorId), ct);

    public Task WithdrawAsync(Guid id, Guid moderatorId, string reason, CancellationToken ct)
        => http.SendJsonAsync(
            HttpMethod.Post, $"/api/moderation/listings/{id}/withdraw", new ModeratorReasonRequest(moderatorId, reason), ct);
}
