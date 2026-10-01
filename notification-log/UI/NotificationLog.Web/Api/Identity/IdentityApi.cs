using Microsoft.Extensions.DependencyInjection.Extensions;
using NotificationLog.Web.Api.Identity.Users;
using NotificationLog.Web.Authentication;

namespace NotificationLog.Web.Api.Identity;

internal static class IdentityApi
{
    private const string BaseAddress = "https+http://identity";

    public static IServiceCollection AddIdentityApi(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddTransient<AccessTokenHandler>();

        services.AddHttpClient(nameof(IdentityApi), client => client.BaseAddress = new(BaseAddress))
            .AddHttpMessageHandler<AccessTokenHandler>()
            .AddTypedClient<UsersApiClient>();

        return services;
    }
}
