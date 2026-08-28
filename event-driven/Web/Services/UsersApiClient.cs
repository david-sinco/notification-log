using static Web.Services.ApiRequest;

namespace Web.Services;

/// <summary>Thin wrapper over Users.Api's 11 endpoints, one method each.</summary>
public sealed class UsersApiClient(HttpClient http)
{
    public Task<ApiResult<UserDto>> RegisterAsync(string name, CancellationToken ct) =>
        SendAsync<UserDto>(http, HttpMethod.Post, "/api/users", new { name }, ct);

    public Task<ApiResult<VerificationTokenDto>> RequestEmailChangeAsync(Guid id, string email, CancellationToken ct) =>
        SendAsync<VerificationTokenDto>(http, HttpMethod.Post, $"/api/users/{id}/email", new { email }, ct);

    public Task<ApiResult<UserDto>> VerifyEmailAsync(Guid id, string token, CancellationToken ct) =>
        SendAsync<UserDto>(http, HttpMethod.Post, $"/api/users/{id}/email/verify", new { token }, ct);

    public Task<ApiResult<VerificationTokenDto>> RequestPhoneChangeAsync(Guid id, string phone, CancellationToken ct) =>
        SendAsync<VerificationTokenDto>(http, HttpMethod.Post, $"/api/users/{id}/phone", new { phone }, ct);

    public Task<ApiResult<UserDto>> VerifyPhoneAsync(Guid id, string token, CancellationToken ct) =>
        SendAsync<UserDto>(http, HttpMethod.Post, $"/api/users/{id}/phone/verify", new { token }, ct);

    public Task<ApiResult<UserDto>> ChangePreferencesAsync(
        Guid id, bool email, bool sms, TimeOnly? quietHoursStart, TimeOnly? quietHoursEnd, CancellationToken ct) =>
        SendAsync<UserDto>(http, HttpMethod.Put, $"/api/users/{id}/preferences",
            new { email, sms, quietHoursStart, quietHoursEnd }, ct);

    public Task<ApiResult<UserDto>> DeactivateAsync(Guid id, CancellationToken ct) =>
        SendAsync<UserDto>(http, HttpMethod.Post, $"/api/users/{id}/deactivate", null, ct);

    public Task<ApiResult<UserDto>> ReactivateAsync(Guid id, CancellationToken ct) =>
        SendAsync<UserDto>(http, HttpMethod.Post, $"/api/users/{id}/reactivate", null, ct);

    public Task<ApiResult<UserDto>> GetProfileAsync(Guid id, CancellationToken ct) =>
        SendAsync<UserDto>(http, HttpMethod.Get, $"/api/users/{id}", null, ct);

    public Task<ApiResult<List<EventEntryDto>>> GetEventsAsync(Guid id, CancellationToken ct) =>
        SendAsync<List<EventEntryDto>>(http, HttpMethod.Get, $"/api/users/{id}/events", null, ct);

    public Task<ApiResult<List<FunnelBucketDto>>> GetFunnelAsync(CancellationToken ct) =>
        SendAsync<List<FunnelBucketDto>>(http, HttpMethod.Get, "/api/users/funnel", null, ct);
}
