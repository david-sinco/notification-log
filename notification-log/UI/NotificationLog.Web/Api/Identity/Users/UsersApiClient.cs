using System.Net.Http.Json;

namespace NotificationLog.Web.Api.Identity.Users;

public sealed class UsersApiClient(HttpClient http)
{
    public async Task<IReadOnlyList<UserDto>> SearchAsync(string? search, int skip, int take, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("search", search),
            ("skip", skip.ToString()),
            ("take", take.ToString()));

        var response = await http.GetAsync($"/api/users{query}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<IReadOnlyList<UserDto>>(ApiJson.Options, ct))!;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var response = await http.GetAsync($"/api/users/{id}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<UserDto>(ApiJson.Options, ct))!;
    }

    public async Task SetRolesAsync(Guid id, SetUserRolesRequest request, CancellationToken ct)
    {
        var response = await http.PutAsJsonAsync($"/api/users/{id}/roles", request, ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
    }

    public async Task LockAsync(Guid id, LockUserRequest request, CancellationToken ct)
    {
        var response = await http.PostAsJsonAsync($"/api/users/{id}/lock", request, ApiJson.Options, ct);
        await response.EnsureSuccessAsync(ct);
    }

    public async Task UnlockAsync(Guid id, CancellationToken ct)
    {
        var response = await http.PostAsync($"/api/users/{id}/unlock", null, ct);
        await response.EnsureSuccessAsync(ct);
    }
}
