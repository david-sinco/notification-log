using static Web.Services.ApiRequest;

namespace Web.Services;

/// <summary>Thin wrapper over Notifications.Api's 4 endpoints (SPEC.md §8).</summary>
public sealed class NotificationsApiClient(HttpClient http)
{
    public Task<ApiResult<NotificationDto>> RequestNotificationAsync(
        Guid userId, ChannelDto channel, string idempotencyKey, string body, CancellationToken ct) =>
        SendAsync<NotificationDto>(http, HttpMethod.Post, "/api/notifications",
            new { userId, channel, idempotencyKey, body }, ct);

    public Task<ApiResult<NotificationDto>> GetByIdAsync(Guid id, CancellationToken ct) =>
        SendAsync<NotificationDto>(http, HttpMethod.Get, $"/api/notifications/{id}", null, ct);

    public Task<ApiResult<ReplicaStatusDto>> GetReplicaStatusAsync(CancellationToken ct) =>
        SendAsync<ReplicaStatusDto>(http, HttpMethod.Get, "/api/notifications/replica/status", null, ct);

    public Task<ApiResult<UserContactDto>> GetReplicaByUserAsync(Guid userId, CancellationToken ct) =>
        SendAsync<UserContactDto>(http, HttpMethod.Get, $"/api/notifications/replica/{userId}", null, ct);
}
