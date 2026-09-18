using Domain.Shared.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace NotificationLog.RentalService.Api.OpenApi;

internal sealed class OAuthSecuritySchemeTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public const string SchemeName = "OAuth2";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var issuer = configuration["Oidc:Issuer"]!.TrimEnd('/');

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri($"{issuer}/connect/authorize"),
                    TokenUrl = new Uri($"{issuer}/connect/token"),
                    Scopes = new Dictionary<string, string>
                    {
                        ["openid"] = "Identidad del usuario",
                        ["roles"] = "Roles del usuario",
                        [OidcScopes.Rentals] = "API de Arriendos"
                    }
                }
            }
        };

        document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(SchemeName, document)] = [] }];

        return Task.CompletedTask;
    }
}
