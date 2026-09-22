using Domain.Shared.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Validation.AspNetCore;

namespace API.Shared.Extensions;

public static class AuthorizationExtensions
{
    public const string AdministradorPolicy = nameof(UserRole.Administrador);
    public const string ModeracionPolicy = nameof(UserRole.Moderador);

    public static IServiceCollection AddOpenIdDictAuthorization(
        this IServiceCollection services, IConfiguration configuration, IReadOnlyList<OidcScope> scopesPolicy)
    {
        var issuer = configuration["Oidc:Issuer"];
        var audience = configuration["Oidc:Audience"];

        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
            throw new InvalidOperationException("Faltan 'Oidc:Issuer' y 'Oidc:Audience' en la configuración.");

        services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

        services.AddOpenIddict()
            .AddValidation(options =>
            {
                options.SetIssuer(new Uri(issuer));
                options.AddAudiences(audience);
                options.UseSystemNetHttp();
                options.UseAspNetCore();
            });

        return services.AddScopePolicies(scopesPolicy);
    }

    public static IServiceCollection AddScopePolicies(
        this IServiceCollection services, IReadOnlyList<OidcScope> scopesPolicy)
    {
        if (scopesPolicy.Count == 0)
            throw new InvalidOperationException("Falta la configuración de scopes en la configuración del servicio.");

        var builder = services.AddAuthorizationBuilder();

        foreach (var scope in scopesPolicy)
        {
            var scopeName = scope.ToScopeName();

            builder.AddPolicy(scopeName, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.HasScope(scopeName)));
        }

        builder.AddPolicy(AdministradorPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.IsAdministrador()))
            .AddPolicy(ModeracionPolicy, policy => policy
                .RequireAuthenticatedUser()
                .RequireAssertion(context => context.User.IsAdministrador() || context.User.IsModerador()));

        return services;
    }
}
