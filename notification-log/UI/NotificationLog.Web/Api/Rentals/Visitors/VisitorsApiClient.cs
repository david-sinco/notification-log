namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed class VisitorsApiClient(HttpClient http)
{
    public Task<PagedResult<VisitorDto>> ListAsync(int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<VisitorDto>>($"/api/visitors{query}", ct);
    }
}
