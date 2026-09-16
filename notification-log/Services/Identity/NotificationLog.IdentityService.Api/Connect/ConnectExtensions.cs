using NotificationLog.IdentityService.Api.Accounts;
using NotificationLog.IdentityService.Api.Data;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Api.Connect;

public static class ConnectExtensions
{
    public const string AdministradorPolicy = nameof(UserRole.Administrador);

    public static IServiceCollection AddConnect(this IServiceCollection services)
    {
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
                options.AddAudiences(OidcScopes.IdentityAudience);
                options.UseAspNetCore();
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(AdministradorPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireRole(nameof(UserRole.Administrador)));

        services.AddScoped<SessionRevoker>();

        return services;
    }
}
