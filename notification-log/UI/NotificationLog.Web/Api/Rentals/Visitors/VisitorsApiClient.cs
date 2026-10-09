namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed class VisitorsApiClient(HttpClient http)
{
    public Task<PagedResult<VisitorDto>> ListAsync(
        string? search, bool? hasVisits, VisitorSort sort, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("search", search),
            ("hasVisits", hasVisits?.ToString().ToLowerInvariant()),
            ("sort", sort.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<VisitorDto>>($"/api/visitors{query}", ct);
    }

    public Task<VisitorsSummaryDto> GetSummaryAsync(CancellationToken ct)
        => http.GetJsonAsync<VisitorsSummaryDto>("/api/visitors/summary", ct);

    public Task<VisitorDetailDto> GetByIdAsync(Guid id, CancellationToken ct)
        => http.GetJsonAsync<VisitorDetailDto>($"/api/visitors/{id}", ct);
}
