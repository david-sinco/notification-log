using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationLog.NotificationService.Application.Notifications.Services.Sending;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Sms;

// Contra el SMS Gateway de Buggregator en dev: autodetecta el proveedor a partir de campos
// genéricos (from/to/message), así que no hace falta imitar la forma exacta de un proveedor real
// para probar el envío. Contra un proveedor real en otros entornos, cambiando solo
// Notifications:Sms en appsettings.
public sealed class HttpSmsNotificationSender : ISmsNotificationSender
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] MessageIdProperties = ["id", "sid", "messageId", "message_id"];

    private readonly HttpClient _client;
    private readonly SmsNotificationOptions _options;

    public HttpSmsNotificationSender(HttpClient client, IOptions<SmsNotificationOptions> options)
        => (_client, _options) = (client, options.Value);

    public async Task<NotificationSendResult> SendAsync(
        string destination, RenderedMessage message, CancellationToken ct)
    {
        var response = await _client.PostAsJsonAsync(
            _options.Path,
            new SendSmsRequest(_options.From, destination, message.Body),
            JsonOptions,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            return NotificationSendResult.Failure($"HTTP {(int)response.StatusCode}: {errorBody}");
        }

        var body = await response.Content.ReadAsStringAsync(ct);
        return NotificationSendResult.Success(TryReadMessageId(body));
    }

    // El shape de la respuesta 2xx no está documentado por Buggregator — se intenta leer un id
    // conocido si viene, pero su ausencia o una forma inesperada no convierte en falla un envío
    // que el gateway ya aceptó.
    private static string? TryReadMessageId(string body)
    {
        if (string.IsNullOrWhiteSpace(body)) return null;

        try
        {
            using var document = JsonDocument.Parse(body);

            foreach (var propertyName in MessageIdProperties)
                if (document.RootElement.TryGetProperty(propertyName, out var value) &&
                    value.ValueKind == JsonValueKind.String)
                    return value.GetString();
        }
        catch (JsonException)
        {
        }

        return null;
    }

    private sealed record SendSmsRequest(string From, string To, string Message);
}
