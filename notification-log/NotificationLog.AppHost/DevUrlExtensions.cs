namespace NotificationLog.AppHost;

public static class DevUrlExtensions
{
    public static IResourceBuilder<T> WithDevUrls<T>(this IResourceBuilder<T> resource)
        where T : IResourceWithEndpoints =>
        resource.WithUrls(context =>
        {
            foreach (var url in context.Urls.Where(url => url.Endpoint is not null))
                url.Url = new UriBuilder(url.Url) { Host = $"{context.Resource.Name}.dev.localhost" }.ToString();
        });
}
