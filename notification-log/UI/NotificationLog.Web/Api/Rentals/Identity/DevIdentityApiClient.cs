namespace NotificationLog.Web.Api.Rentals.Identity;

public sealed class DevIdentityApiClient(HttpClient http)
{
    public Task<IdentitySnapshotDto> GetAsync(CancellationToken ct)
        => http.GetJsonAsync<IdentitySnapshotDto>("/api/dev/identity", ct);

    public Task SetPersonAsync(Guid personId, SetPersonRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Put, $"/api/dev/identity/people/{personId}", request, ct);

    public Task SetAdvisorAsync(Guid advisorId, SetAdvisorRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Put, $"/api/dev/identity/advisors/{advisorId}", request, ct);
}
