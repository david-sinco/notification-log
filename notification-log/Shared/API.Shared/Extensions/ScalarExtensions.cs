using API.Shared.OpenApi;
using Domain.Shared.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace API.Shared.Extensions;

public static class ScalarExtensions
{
    public static IServiceCollection AddOAuthOpenApi(this IServiceCollection services, IReadOnlyList<OidcScope> scopes)
    {
        services.AddSingleton(new OAuthScalarScopes(scopes));

        return services.AddOpenApi(options => options.AddDocumentTransformer<OAuthSecuritySchemeTransformer>());
    }

    public static WebApplication MapOAuthScalar(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        var scopes = app.Services.GetRequiredService<OAuthScalarScopes>();

        app.MapOpenApi();
        app.MapScalarApiReference((options, context) => options
            .AddPreferredSecuritySchemes(OAuthSecuritySchemeTransformer.SchemeName)
            .AddAuthorizationCodeFlow(OAuthSecuritySchemeTransformer.SchemeName, flow => flow
                .WithClientId(app.Configuration["Scalar:ClientId"])
                .WithPkce(Pkce.Sha256)
                .WithRedirectUri($"{context.Request.Scheme}://{context.Request.Host}/scalar/")
                .WithSelectedScopes(scopes.Names)));

        return app;
    }

    public static WebApplication MapRootToScalar(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return app;

        app.MapGet("/", () => Results.Redirect("/scalar/")).ExcludeFromDescription();

        return app;
    }
}
