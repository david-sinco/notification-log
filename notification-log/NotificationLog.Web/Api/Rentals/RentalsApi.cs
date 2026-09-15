using System.Net.Http.Json;

namespace NotificationLog.Web.Api.Rentals;

internal static class RentalsApi
{
    public const string BaseAddress = "https+http://notificationlog-rentalservice-api";

    public static async Task<T> GetJsonAsync<T>(this HttpClient http, string url, CancellationToken ct)
    {
        var response = await http.GetAsync(url, ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, ct))!;
    }

    public static async Task<HttpResponseMessage> SendJsonAsync(
        this HttpClient http, HttpMethod method, string url, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);

        if (body is not null)
            request.Content = JsonContent.Create(body, body.GetType(), options: ApiJson.Options);

        var response = await http.SendAsync(request, ct);
        await response.EnsureSuccessAsync(ct);
        return response;
    }

    public static async Task<Guid> SendForIdAsync(
        this HttpClient http, HttpMethod method, string url, object body, CancellationToken ct)
    {
        var response = await http.SendJsonAsync(method, url, body, ct);
        var created = await response.Content.ReadFromJsonAsync<CreatedResponse>(ApiJson.Options, ct);
        return created!.Id;
    }
}
