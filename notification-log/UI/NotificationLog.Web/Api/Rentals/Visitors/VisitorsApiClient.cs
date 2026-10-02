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

    public Task<VisitorDto> GetMeAsync(CancellationToken ct)
        => http.GetJsonAsync<VisitorDto>("/api/visitors/me", ct);

    public async Task CompleteProfileAsync(CompleteVisitorProfileRequest request, CancellationToken ct)
        => (await http.SendJsonAsync(HttpMethod.Put, "/api/visitors/me/profile", request, ct)).Dispose();
}
