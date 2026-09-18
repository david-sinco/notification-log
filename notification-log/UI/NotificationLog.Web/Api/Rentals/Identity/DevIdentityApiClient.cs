namespace NotificationLog.Web.Api.Rentals.Identity;

public sealed class DevIdentityApiClient(HttpClient http)
{
    public Task<IdentitySnapshotDto> GetAsync(CancellationToken ct)
        => http.GetJsonAsync<IdentitySnapshotDto>("/api/dev/identity", ct);

    public Task SetPersonAsync(Guid personId, SetPersonRequest request, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Put, $"/api/dev/identity/people/{personId}", request, ct);
}
