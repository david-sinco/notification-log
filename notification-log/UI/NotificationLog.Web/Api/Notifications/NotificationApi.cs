using NotificationLog.Web.Api.Recipients;
using NotificationLog.Web.Api.Templates;
using NotificationLog.Web.Api.Triggers;

namespace NotificationLog.Web.Api.Notifications;

internal static class NotificationApi
{
    // La URL usa "https+http://" para preferir HTTPS cuando esté disponible; la resuelve el
    // descubrimiento de servicios de Aspire.
    private const string BaseAddress = "https+http://notification";

    public static IServiceCollection AddNotificationApi(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(NotificationApi), client => client.BaseAddress = new(BaseAddress))
            .AddTypedClient<TriggersApiClient>()
            .AddTypedClient<TemplatesApiClient>()
            .AddTypedClient<RecipientsApiClient>()
            .AddTypedClient<NotificationsApiClient>();

        return services;
    }
}
