using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace NotificationLog.Web.Authentication;

public sealed class AccessTokenHandler(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (httpContextAccessor.HttpContext is { } context && await context.GetTokenAsync("access_token") is { } token)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }
}
