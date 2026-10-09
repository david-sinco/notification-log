using Domain.Shared.Authorization;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace API.Shared.OpenApi;

internal sealed class OAuthScalarScopes(IReadOnlyList<OidcScope> apiScopes)
{
    private static readonly Dictionary<OidcScope, string> ApiDescriptions = new()
    {
        [OidcScope.Identity] = "API de Identidad",
        [OidcScope.Rentals] = "API de Arriendos",
        [OidcScope.Notifications] = "API de Notificaciones"
    };

    public IReadOnlyDictionary<string, string> Descriptions { get; } = new Dictionary<string, string>
        {
            [Scopes.OpenId] = "Identidad del usuario",
            [Scopes.Roles] = "Roles del usuario"
        }
        .Concat(apiScopes.Select(scope => KeyValuePair.Create(scope.ToScopeName(), ApiDescriptions[scope])))
        .ToDictionary();

    public string[] Names => [.. Descriptions.Keys];
}
