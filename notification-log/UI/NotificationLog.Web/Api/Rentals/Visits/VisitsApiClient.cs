namespace NotificationLog.Web.Api.Rentals.Visits;

public sealed class VisitsApiClient(HttpClient http)
{
    public Task<PagedResult<VisitDto>> ListAsync(
        Guid? listingId, Guid? participantId, VisitStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("listingId", listingId?.ToString()),
            ("participantId", participantId?.ToString()),
            ("status", status?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<VisitDto>>($"/api/visits{query}", ct);
    }

    public Task<VisitDto> GetByIdAsync(Guid id, CancellationToken ct)
        => http.GetJsonAsync<VisitDto>($"/api/visits/{id}", ct);

    public Task<Guid> RequestAsync(RequestVisitRequest request, CancellationToken ct)
        => http.SendForIdAsync(HttpMethod.Post, "/api/visits", request, ct);

    public Task CounterProposeAsync(Guid id, IReadOnlyList<DateTimeOffset> slots, CancellationToken ct)
        => http.SendJsonAsync(
            HttpMethod.Post, $"/api/visits/{id}/counter-proposal", new CounterProposeVisitRequest(slots), ct);

    public Task ScheduleAsync(Guid id, DateTimeOffset startsAt, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/visits/{id}/schedule", new ScheduleVisitRequest(startsAt), ct);

    public Task CancelAsync(Guid id, string reason, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/visits/{id}/cancel", new CancelVisitRequest(reason), ct);

    public Task MarkCompletedAsync(Guid id, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/visits/{id}/complete", null, ct);

    public Task MarkNoShowAsync(Guid id, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/visits/{id}/no-show", null, ct);
}
