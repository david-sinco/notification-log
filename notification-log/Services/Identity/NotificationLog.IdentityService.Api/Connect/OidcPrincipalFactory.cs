using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using NotificationLog.IdentityService.Api.Accounts;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Api.Connect;

public static class OidcPrincipalFactory
{
    public static ClaimsPrincipal CreateSession(SignedInUser user)
    {
        var identity = new ClaimsIdentity(SessionCookie.Scheme, Claims.Name, Claims.Role);

        identity
            .SetClaim(Claims.Subject, user.Id.ToString())
            .SetClaim(Claims.Name, user.DisplayName)
            .SetClaim(OidcClaims.SecurityStamp, user.SecurityStamp);

        return new ClaimsPrincipal(identity);
    }

    public static ClaimsPrincipal CreateToken(SignedInUser user, ImmutableArray<string> scopes, IEnumerable<string> resources)
    {
        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType, Claims.Name, Claims.Role);

        identity
            .SetClaim(Claims.Subject, user.Id.ToString())
            .SetClaim(Claims.Name, user.DisplayName)
            .SetClaim(Claims.Email, user.Email)
            .SetClaim(Claims.PhoneNumber, user.Phone)
            .SetClaim(OidcClaims.SecurityStamp, user.SecurityStamp)
            .SetClaims(Claims.Role, [.. user.Roles]);

        identity.SetScopes(scopes);
        identity.SetResources(resources);
        identity.SetDestinations(GetDestinations);

        return new ClaimsPrincipal(identity);
    }

    private static IEnumerable<string> GetDestinations(Claim claim) => claim.Type switch
    {
        OidcClaims.SecurityStamp => [],

        Claims.Name => [Destinations.AccessToken, Destinations.IdentityToken],

        Claims.Email when claim.Subject!.HasScope(Scopes.Email)
            => [Destinations.AccessToken, Destinations.IdentityToken],

        Claims.PhoneNumber when claim.Subject!.HasScope(Scopes.Phone)
            => [Destinations.AccessToken, Destinations.IdentityToken],

        Claims.Role when claim.Subject!.HasScope(Scopes.Roles)
            => [Destinations.AccessToken, Destinations.IdentityToken],

        _ => [Destinations.AccessToken]
    };
}
