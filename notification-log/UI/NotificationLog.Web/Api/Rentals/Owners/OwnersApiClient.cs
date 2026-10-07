namespace NotificationLog.Web.Api.Rentals.Owners;

public sealed class OwnersApiClient(HttpClient http)
{
    public Task<PagedResult<OwnerDto>> ListAsync(Guid? createdBy, string? search, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("createdBy", createdBy?.ToString()),
            ("search", search),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        return http.GetJsonAsync<PagedResult<OwnerDto>>($"/api/owners{query}", ct);
    }

    public Task<IReadOnlyList<OwnerDto>> ListMineAsync(CancellationToken ct)
        => http.GetJsonAsync<IReadOnlyList<OwnerDto>>("/api/owners/mine", ct);

    public Task<IReadOnlyList<OwnerDto>> ListClaimableAsync(CancellationToken ct)
        => http.GetJsonAsync<IReadOnlyList<OwnerDto>>("/api/owners/claimable", ct);

    public Task ClaimAsync(Guid id, CancellationToken ct)
        => http.SendJsonAsync(HttpMethod.Post, $"/api/owners/{id}/claim", null, ct);

    public Task<OwnerDto> GetByIdAsync(Guid id, CancellationToken ct)
        => http.GetJsonAsync<OwnerDto>($"/api/owners/{id}", ct);

    public Task<Guid> RegisterAsync(RegisterOwnerRequest request, CancellationToken ct)
        => http.SendForIdAsync(HttpMethod.Post, "/api/owners", request, ct);
}
