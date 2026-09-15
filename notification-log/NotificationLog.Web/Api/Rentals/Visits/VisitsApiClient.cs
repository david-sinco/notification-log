namespace NotificationLog.Web.Api.Rentals.Visits;

public sealed class VisitsApiClient(HttpClient http)
{
    public Task<PagedResult<VisitSummaryDto>> ListAsync(
        Guid? listingId, Guid? participantId, VisitStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("listingId", listingId?.ToString()),
            ("participantId", participantId?.ToString()),
            ("status", status?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<VisitSummaryDto>>($"/api/visits{query}", ct);
    }

    public Task<VisitDto> GetByIdAsync(Guid id, CancellationToken ct)
        => http.GetJsonAsync<VisitDto>($"/api/visits/{id}", ct);

    public Task<Guid> RequestAsync(RequestVisitRequest request, CancellationToken ct)
        => http.SendForIdAsync(HttpMethod.Post, "/api/visits", request, ct);

    public Task ConfirmAsync(Guid id, Guid actorId, DateTimeOffset slotStart, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/visits/{id}/confirm", new ConfirmVisitRequest(actorId, slotStart), ct);

    public Task DeclineAsync(Guid id, Guid actorId, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/visits/{id}/decline", new VisitReasonRequest(actorId, reason), ct);

    public Task CancelAsync(Guid id, Guid actorId, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/visits/{id}/cancel", new VisitReasonRequest(actorId, reason), ct);

    public Task ReportOutcomeAsync(Guid id, Guid actorId, bool attended, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/visits/{id}/outcome", new ReportVisitOutcomeRequest(actorId, attended), ct);
}
