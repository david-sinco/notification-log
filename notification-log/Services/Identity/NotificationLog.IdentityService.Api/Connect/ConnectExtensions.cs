using Domain.Shared.Authorization;
using NotificationLog.IdentityService.Infrastructure.Security;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Api.Connect;

public static class ConnectExtensions
{
    public static IServiceCollection AddConnect(this IServiceCollection services, IConfiguration configuration)
    {
        var oidc = configuration.GetSection(OidcOptions.SectionName).Get<OidcOptions>();
        var identityScope = OidcScope.Identity.ToScopeName();

        if (string.IsNullOrWhiteSpace(oidc?.Issuer) || !oidc.Audiences.TryGetValue(identityScope, out var identityAudience))
            throw new InvalidOperationException(
                $"Faltan 'Oidc:Issuer' y 'Oidc:Audiences:{identityScope}' en la configuración.");

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
            .AddCookie(SessionCookie.Scheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.Cookie.Name = "notificationlog.identity";
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
            });

        services.AddOpenIddict()
            .AddServer(options =>
            {
                options.SetIssuer(new Uri(oidc.Issuer));

                options.SetAuthorizationEndpointUris("connect/authorize")
                    .SetTokenEndpointUris("connect/token")
                    .SetEndSessionEndpointUris("connect/endsession");

                options.AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow()
                    .RequireProofKeyForCodeExchange();

                options.RegisterScopes(
                    Scopes.Email,
                    Scopes.Phone,
                    Scopes.Roles,
                    Scopes.OfflineAccess,
                    OidcScope.Identity.ToScopeName(),
                    OidcScope.Rentals.ToScopeName(),
                    OidcScope.Notifications.ToScopeName());

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));

                options.AddDevelopmentEncryptionCertificate()
                    .AddDevelopmentSigningCertificate();

                options.DisableAccessTokenEncryption();

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.AddAudiences(identityAudience);
                options.UseAspNetCore();
            });

        return services;
    }
}
