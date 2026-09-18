using Domain.Shared.Authorization;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace NotificationLog.RentalService.Api.Authorization;

public static class AuthorizationExtensions
{
    public const string RentalsScopePolicy = OidcScopes.Rentals;
    public const string AdministradorPolicy = nameof(UserRole.Administrador);
    public const string AsesorPolicy = nameof(UserRole.Asesor);

    public static IServiceCollection AddRentalsAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Oidc:Issuer"];
        var audience = configuration[$"Oidc:Audiences:{OidcScopes.Rentals}"];

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Faltan 'Oidc:Issuer' y 'Oidc:Audiences:rentals' en la configuración.");

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.SetIssuer(new Uri(issuer));
                options.AddAudiences(audience);
                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(RentalsScopePolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.HasScope(OidcScopes.Rentals)))
            .AddPolicy(AdministradorPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.IsAdministrador()))
            .AddPolicy(AsesorPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.IsAsesor()));

        return services;
    }
}
