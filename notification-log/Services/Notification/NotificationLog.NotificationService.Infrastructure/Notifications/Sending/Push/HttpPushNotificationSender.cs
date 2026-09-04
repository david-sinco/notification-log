using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NotificationLog.NotificationService.Application.Notifications.Services.Sending;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Push;

// Adaptador REST genérico: ningún proveedor concreto de push (FCM, OneSignal, APNs) está elegido
// todavía, así que el shape del request/response es el mínimo común — cambiar de proveedor implica
// cambiar esta clase, no el puerto IPushNotificationSender ni NotificationDispatchService.
public sealed class HttpPushNotificationSender : IPushNotificationSender
{
    private static readonly JsonSerializerOptions ResponseJsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly HttpClient _client;
    private readonly PushNotificationOptions _options;

    public HttpPushNotificationSender(HttpClient client, IOptions<PushNotificationOptions> options)
        => (_client, _options) = (client, options.Value);

    public async Task<NotificationSendResult> SendAsync(
        string destination, RenderedMessage message, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.Path)
        {
            Content = JsonContent.Create(new SendPushRequest(
                destination, message.Subject, message.Body))
        };

        if (!string.IsNullOrEmpty(_options.ApiKey))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        var response = await _client.SendAsync(request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            return NotificationSendResult.Failure($"HTTP {(int)response.StatusCode}: {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<SendPushResponse>(
            ResponseJsonOptions, ct);

        return NotificationSendResult.Success(payload?.Id);
    }

    private sealed record SendPushRequest(string To, string? Title, string Body);

    private sealed record SendPushResponse(string? Id);
}
