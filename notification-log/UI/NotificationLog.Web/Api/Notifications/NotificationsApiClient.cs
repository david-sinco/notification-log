using System.Net.Http.Json;

namespace NotificationLog.Web.Api.Notifications;

public sealed class NotificationsApiClient(HttpClient http)
{
    public async Task<PagedResult<NotificationDto>> ListAsync(
        Guid? recipientId, string? search, DeliveryStatus? status, NotificationChannel? channel, int page, int pageSize, CancellationToken ct)
    {
        var query = QueryString.Build(
            ("recipientId", recipientId?.ToString()),
            ("search", search),
            ("channel", channel?.ToString()),
            ("status", status?.ToString()),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()));

        var response = await http.GetAsync($"/api/notifications{query}", ct);
        await response.EnsureSuccessAsync(ct);
        return (await response.Content.ReadFromJsonAsync<PagedResult<NotificationDto>>(ApiJson.Options, ct))!;
    }
}
