namespace NotificationLog.Web.Api.Rentals.Owners;

public sealed class OwnersApiClient(HttpClient http)
{
    public Task<PagedResult<OwnerDto>> ListAsync(Guid? createdBy, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("createdBy", createdBy?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<OwnerDto>>($"/api/owners{query}", ct);
    }

    public Task<OwnerDto> GetByIdAsync(Guid id, CancellationToken ct)
        => http.GetJsonAsync<OwnerDto>($"/api/owners/{id}", ct);

    public Task<Guid> RegisterNaturalAsync(RegisterNaturalOwnerRequest request, CancellationToken ct)
        => http.SendForIdAsync(HttpMethod.Post, "/api/owners/natural", request, ct);

    public Task<Guid> RegisterCompanyAsync(RegisterCompanyOwnerRequest request, CancellationToken ct)
        => http.SendForIdAsync(HttpMethod.Post, "/api/owners/company", request, ct);
}
