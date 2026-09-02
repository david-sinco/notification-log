using System.Net.Http.Json;

namespace NotificationLog.Web.Api.Triggers;

public sealed class TriggersApiClient(HttpClient http)
{
    public async Task<PagedResult<TriggerSummaryDto>> ListAsync(
        string? search, bool? isEnabled, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("search", search),
            ("isEnabled", isEnabled?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        var response = await http.GetAsync($"/api/triggers{query}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<PagedResult<TriggerSummaryDto>>(ApiJson.Options, ct))!;
    }

    public async Task<TriggerDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await http.GetAsync($"/api/triggers/{id}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<TriggerDto>(ApiJson.Options, ct))!;
    }

    public async Task<Guid> CreateAsync(CreateTriggerRequest request, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync("/api/triggers", request, ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
        var body = await response.Content.ReadFromJsonAsync<CreatedResponse>(ApiJson.Options, ct);
        return body!.Id;
    }

    public async Task SetStatusAsync(Guid id, bool isEnabled, CancellationToken ct)
    {
        var response = await http.PatchAsJsonAsync(
            $"/api/triggers/{id}/status", new SetTriggerStatusRequest(isEnabled), ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
    }

    public async Task UpdateDescriptionAsync(Guid id, string description, CancellationToken ct)
    {
        var response = await http.PatchAsJsonAsync(
            $"/api/triggers/{id}/description", new UpdateTriggerDescriptionRequest(description), ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
    }

    public async Task<Guid> AddConfigurationAsync(
        Guid id, Guid templateId, NotificationChannel channel, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync(
            $"/api/triggers/{id}/configurations", new AddConfigurationRequest(templateId, channel), ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
        var body = await response.Content.ReadFromJsonAsync<CreatedResponse>(ApiJson.Options, ct);
        return body!.Id;
    }

    public async Task DisableConfigurationAsync(Guid id, Guid configId, CancellationToken ct)
    {
        var response = await http.DeleteAsync($"/api/triggers/{id}/configurations/{configId}", ct);
        await response.EnsureSuccessAsync(ct);
    }

    public async Task EnableConfigurationAsync(Guid id, Guid configId, CancellationToken ct)
    {
        var response = await http.PutAsync($"/api/triggers/{id}/configurations/{configId}/enable", null, ct);
        await response.EnsureSuccessAsync(ct);
    }

    public async Task ChangeConfigurationTemplateAsync(Guid id, Guid configId, Guid templateId, CancellationToken ct)
    {
        var response = await http.PutAsJsonAsync(
            $"/api/triggers/{id}/configurations/{configId}/template",
            new ChangeConfigurationTemplateRequest(templateId), ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
    }
}
