using System.Net.Http.Json;

namespace NotificationLog.Web.Api.Notifications;

public sealed class NotificationsApiClient(HttpClient http)
{
    public async Task<PagedResult<NotificationDto>> ListAsync(
        Guid? recipientId, string? eventKey, DeliveryStatus? status, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("recipientId", recipientId?.ToString()),
            ("eventKey", eventKey),
            ("status", status?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        var response = await http.GetAsync($"/api/notifications{query}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<PagedResult<NotificationDto>>(ApiJson.Options, ct))!;
    }
}
