using System.Net.Http.Json;

namespace NotificationLog.Web.Api.Recipients;

public sealed class RecipientsApiClient(HttpClient http)
{
    public async Task<PagedResult<RecipientSummaryDto>> ListAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("search", search),
            ("isActive", isActive?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        var response = await http.GetAsync($"/api/recipients{query}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<PagedResult<RecipientSummaryDto>>(ApiJson.Options, ct))!;
    }

    public async Task<RecipientDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await http.GetAsync($"/api/recipients/{id}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<RecipientDto>(ApiJson.Options, ct))!;
    }
}
