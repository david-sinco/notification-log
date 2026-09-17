using NotificationLog.IdentityService.Api.Accounts;
using NotificationLog.IdentityService.Api.Data;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Api.Connect;

public static class ConnectExtensions
{
    public const string IdentityScopePolicy = OidcScopes.Identity;
    public const string AdministradorPolicy = nameof(UserRole.Administrador);

    public static IServiceCollection AddConnect(this IServiceCollection services, IConfiguration configuration)
    {
        var oidcSection = configuration.GetSection(OidcOptions.SectionName);
        var oidc = oidcSection.Get<OidcOptions>();

        if (string.IsNullOrWhiteSpace(oidc?.Issuer) || !oidc.Audiences.TryGetValue(OidcScopes.Identity, out var identityAudience))
            throw new InvalidOperationException("Faltan 'Oidc:Issuer' y 'Oidc:Audiences:identity' en la configuración.");

        services.Configure<OidcOptions>(oidcSection);

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)
            .AddCookie(SessionCookie.Scheme, options =>
            {
                options.LoginPath = "/Account/Login";
                options.Cookie.Name = "notificationlog.identity";
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
                options.SlidingExpiration = true;
            });

        services.AddOpenIddict()
            .AddCore(options => options
                .UseEntityFrameworkCore()
                .UseDbContext<IdentityServiceDbContext>())
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
                    OidcScopes.Identity,
                    OidcScopes.Rentals,
                    OidcScopes.Notifications);

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

        services.AddAuthorizationBuilder()
            .AddPolicy(IdentityScopePolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.HasScope(OidcScopes.Identity)))
            .AddPolicy(AdministradorPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(nameof(UserRole.Administrador)));

        services.AddScoped<SessionRevoker>();

        return services;
    }
}
