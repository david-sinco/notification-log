using System.Net.Http.Json;
using System.Text.Json;

namespace NotificationLog.Web.Api;

internal static class ApiResponseExtensions
{
    // El ApiService devuelve ProblemDetails/ValidationProblemDetails en los errores (404, 400, 422).
    // Si el cuerpo no es JSON parseable (p. ej. un 500 sin cuerpo), se degrada a un mensaje genérico.
    public static async Task EnsureSuccessAsync(this HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;

        ProblemPayload? problem = null;
        try
        {
            problem = await response.Content.ReadFromJsonAsync<ProblemPayload>(ApiJson.Options, ct);
        }
        catch (JsonException)
        {
        }

        throw new ApiException(response.StatusCode, problem?.Title, problem?.Detail, problem?.Errors);
    }

    private sealed record ProblemPayload(string? Title, string? Detail, Dictionary<string, string[]>? Errors);
}
