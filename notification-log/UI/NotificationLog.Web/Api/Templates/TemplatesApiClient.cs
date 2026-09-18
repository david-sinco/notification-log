using System.Net.Http.Json;

namespace NotificationLog.Web.Api.Templates;

public sealed class TemplatesApiClient(HttpClient http)
{
    public async Task<PagedResult<TemplateSummaryDto>> ListAsync(
        string? search, NotificationChannel? channel, bool? isEnabled, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("search", search),
            ("channel", channel?.ToString()),
            ("isEnabled", isEnabled?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        var response = await http.GetAsync($"/api/templates{query}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<PagedResult<TemplateSummaryDto>>(ApiJson.Options, ct))!;
    }

    public async Task<TemplateDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await http.GetAsync($"/api/templates/{id}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<TemplateDto>(ApiJson.Options, ct))!;
    }

    public async Task<Guid> CreateAsync(CreateTemplateRequest request, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/templates", request, ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
        var body = await response.Content.ReadFromJsonAsync<CreatedResponse>(ApiJson.Options, ct);
        return body!.Id;
    }

    public async Task<Guid> PublishVersionAsync(Guid id, string? subject, string body, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/templates/{id}/versions", new PublishTemplateVersionRequest(subject, body), ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
        var payload = await response.Content.ReadFromJsonAsync<CreatedResponse>(ApiJson.Options, ct);
        return payload!.Id;
    }

    public async Task SetStatusAsync(Guid id, bool isEnabled, CancellationToken ct)
    {
        var response = await http.PatchAsJsonAsync(
            $"/api/templates/{id}/status", new SetTemplateStatusRequest(isEnabled), ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
    }
}
