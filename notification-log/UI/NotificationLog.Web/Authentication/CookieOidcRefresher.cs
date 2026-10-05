using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace NotificationLog.Web.Authentication;

public sealed class CookieOidcRefresher(IOptionsMonitor<OpenIdConnectOptions> oidcOptionsMonitor)
{
    private static readonly TimeSpan RefreshMargin = TimeSpan.FromMinutes(5);

    public async Task ValidateOrRefreshCookieAsync(CookieValidatePrincipalContext context, string oidcScheme)
    {
        var expiresAtText = context.Properties.GetTokenValue("expires_at");

        if (!DateTimeOffset.TryParse(expiresAtText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var expiresAt))
            return;

        var options = oidcOptionsMonitor.Get(oidcScheme);
        var now = options.TimeProvider!.GetUtcNow();

        if (now + RefreshMargin < expiresAt)
            return;

        var ct = context.HttpContext.RequestAborted;
        var configuration = await options.ConfigurationManager!.GetConfigurationAsync(ct);

        using var response = await options.Backchannel.PostAsync(
            configuration.TokenEndpoint,
            new FormUrlEncodedContent(new Dictionary<string, string?>
            {
                ["grant_type"] = "refresh_token",
                ["client_id"] = options.ClientId,
                ["client_secret"] = options.ClientSecret,
                ["refresh_token"] = context.Properties.GetTokenValue("refresh_token")
            }),
            ct);

        if (!response.IsSuccessStatusCode)
        {
            context.RejectPrincipal();
            return;
        }

        var message = new OpenIdConnectMessage(await response.Content.ReadAsStringAsync(ct));
        var newExpiresAt = now + TimeSpan.FromSeconds(int.Parse(message.ExpiresIn, CultureInfo.InvariantCulture));

        context.Properties.UpdateTokenValue("access_token", message.AccessToken);
        context.Properties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o", CultureInfo.InvariantCulture));

        if (!string.IsNullOrEmpty(message.RefreshToken))
            context.Properties.UpdateTokenValue("refresh_token", message.RefreshToken);

        if (!string.IsNullOrEmpty(message.IdToken))
            context.Properties.UpdateTokenValue("id_token", message.IdToken);

        context.ShouldRenew = true;
    }
}
