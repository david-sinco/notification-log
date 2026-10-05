using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationLog.Web.Api.Rentals.Dashboard;
using NotificationLog.Web.Api.Rentals.Listings;
using NotificationLog.Web.Api.Rentals.Owners;
using NotificationLog.Web.Api.Rentals.Visitors;
using NotificationLog.Web.Api.Rentals.Visits;
using NotificationLog.Web.Authentication;

namespace NotificationLog.Web.Api.Rentals;

internal static class RentalsApi
{
    private const string BaseAddress = "https+http://rental";

    public static IServiceCollection AddRentalsApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddTransient<AccessTokenHandler>();

        services.AddHttpClient(nameof(RentalsApi), client => client.BaseAddress = new(BaseAddress))
            .AddHttpMessageHandler<AccessTokenHandler>()
            .AddTypedClient<OwnersApiClient>()
            .AddTypedClient<ListingsApiClient>()
            .AddTypedClient<ModerationApiClient>()
            .AddTypedClient<VisitsApiClient>()
            .AddTypedClient<VisitorsApiClient>()
            .AddTypedClient<DashboardApiClient>();

        return services;
    }

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
