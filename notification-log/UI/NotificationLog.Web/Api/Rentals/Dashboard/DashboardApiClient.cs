namespace NotificationLog.Web.Api.Rentals.Dashboard;

public sealed class DashboardApiClient(HttpClient http)
{
    public Task<DashboardDto> GetAsync(CancellationToken ct)
        => http.GetJsonAsync<DashboardDto>("/api/dashboard", ct);
}
