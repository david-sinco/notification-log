using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace NotificationLog.Web.Authentication;

public static class IdentityAuthenticationExtensions
{
    public static IServiceCollection AddIdentityAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var identity = configuration.GetSection("Identity");

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.Name = "notificationlog.web";
                options.AccessDeniedPath = "/authentication/access-denied";
            })
            .AddOpenIdConnect(options =>
            {
                options.Authority = identity["Authority"]
                    ?? throw new InvalidOperationException("Falta la configuración 'Identity:Authority'.");
                options.BackchannelHttpHandler = LocalhostSubdomainHandler.Create();
                options.ClientId = identity["ClientId"];
                options.ClientSecret = identity["ClientSecret"];

                options.ResponseType = "code";
                options.UsePkce = true;
                options.SaveTokens = true;
                options.MapInboundClaims = false;
                options.GetClaimsFromUserInfoEndpoint = false;

                options.Scope.Clear();
                foreach (var scope in new[] { "openid", "email", "phone", "roles", "offline_access", "identity" })
                    options.Scope.Add(scope);

                options.TokenValidationParameters.NameClaimType = IdentityClaims.Name;
                options.TokenValidationParameters.RoleClaimType = IdentityClaims.Role;
            });

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        return services;
    }
}
