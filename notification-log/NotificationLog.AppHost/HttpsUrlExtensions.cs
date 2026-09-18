namespace NotificationLog.AppHost;

public static class HttpsUrlExtensions
{
    public static IResourceBuilder<T> WithHttpsUrlsOnly<T>(this IResourceBuilder<T> resource)
        where T : IResourceWithEndpoints =>
        resource.WithUrls(context => context.Urls.RemoveAll(url => url.Endpoint?.EndpointName == "http"));
}
