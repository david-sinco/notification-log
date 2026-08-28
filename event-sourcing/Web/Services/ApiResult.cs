using System.Net.Http.Json;
using System.Text.Json;

namespace Web.Services;

/// <summary>
/// Always carries the raw response body alongside the parsed value, on success or failure — the
/// point of this console is to see exactly what the API said, including 422/409/404 error bodies.
/// </summary>
public sealed record ApiResult<T>(bool Success, int StatusCode, T? Value, string RawBody);

internal static class ApiRequest
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
    };

    public static async Task<ApiResult<T>> SendAsync<T>(HttpClient http, HttpMethod method, string url,
        object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body, options: JsonOptions);

        using var response = await http.SendAsync(request, ct);
        var raw = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
            return new ApiResult<T>(false, (int)response.StatusCode, default, raw);

        var value = string.IsNullOrWhiteSpace(raw)
            ? default
            : JsonSerializer.Deserialize<T>(raw, JsonOptions);

        return new ApiResult<T>(true, (int)response.StatusCode, value, raw);
    }
}
