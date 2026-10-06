using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class RentalsClient(RentalsApi api, ScenarioState state)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01];

    private readonly RentalsApi _api = api;
    private readonly ScenarioState _state = state;
    private readonly HttpClient _http = api.CreateClient();

    public Task<HttpResponseMessage> SendAsync(TestUser? user, HttpMethod method, string url, object? body = null)
        => SendAsync(user, method, url, body is null ? null : JsonContent.Create(body, options: Json));

    public Task<HttpResponseMessage> UploadPhotoAsync(TestUser? user, Guid listingId)
    {
        var file = new ByteArrayContent(Jpeg);
        file.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

        return SendAsync(
            user, HttpMethod.Post, $"/api/listings/{listingId}/photos", new MultipartFormDataContent { { file, "file", "foto.jpg" } });
    }

    public async Task<T> SetUpAsync<T>(TestUser? user, HttpMethod method, string url, object? body = null)
    {
        await SetUpAsync(user, method, url, body);

        return await ReadAsync<T>();
    }

    public async Task SetUpAsync(TestUser? user, HttpMethod method, string url, object? body = null)
        => await EnsureSucceededAsync(await SendAsync(user, method, url, body), $"{method} {url}");

    public async Task SetUpPhotoAsync(TestUser? user, Guid listingId)
        => await EnsureSucceededAsync(await UploadPhotoAsync(user, listingId), "subir una foto");

    public async Task<T> QueryAsync<T>(string url)
    {
        using var response = await SendWithoutTrackingAsync(_state.Staff, HttpMethod.Get, url, null);
        await EnsureSucceededAsync(response, $"GET {url}");

        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }

    public async Task<HttpResponseMessage> ProbeAsync(TestUser? user, string url)
        => await SendWithoutTrackingAsync(user, HttpMethod.Get, url, null);

    public async Task<T> ReadAsync<T>()
        => JsonSerializer.Deserialize<T>(await _state.Response!.Content.ReadAsStringAsync(), Json)!;

    public async Task<string?> ReadErrorAsync()
    {
        var body = await _state.Response!.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(body))
            return null;

        using var problem = JsonDocument.Parse(body);

        return problem.RootElement.TryGetProperty("detail", out var detail) ? detail.GetString() : null;
    }

    private async Task<HttpResponseMessage> SendAsync(TestUser? user, HttpMethod method, string url, HttpContent? content)
    {
        _state.NotificationsBeforeLastRequest = _api.Notifications.Sent.Count;
        _state.Response = await SendWithoutTrackingAsync(user, method, url, content);

        return _state.Response;
    }

    private Task<HttpResponseMessage> SendWithoutTrackingAsync(TestUser? user, HttpMethod method, string url, HttpContent? content)
    {
        var request = new HttpRequestMessage(method, url) { Content = content };

        if (user is not null)
        {
            request.Headers.Add(TestAuthHandler.UserHeader, user.Id.ToString());
            request.Headers.Add(TestAuthHandler.RoleHeader, user.Role.ToString());
        }

        return _http.SendAsync(request);
    }

    private static async Task EnsureSucceededAsync(HttpResponseMessage response, string action)
    {
        if (!response.IsSuccessStatusCode)
            Assert.Fail($"La preparación del escenario falló al {action}: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
    }
}
