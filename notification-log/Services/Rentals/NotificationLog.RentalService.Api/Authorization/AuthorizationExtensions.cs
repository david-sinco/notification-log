using Domain.Shared.Authorization;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace NotificationLog.RentalService.Api.Authorization;

public static class AuthorizationExtensions
{
    public const string RentalsScopePolicy = OidcScopes.Rentals;
    public const string AdministradorPolicy = nameof(UserRole.Administrador);
    public const string ModeracionPolicy = "Moderacion";

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
            .AddPolicy(ModeracionPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.IsAdministrador() || context.User.IsModerador()));

        return services;
    }
}
