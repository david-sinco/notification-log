using System.Security.Claims;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Api.Connect;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
        => Guid.TryParse(principal.GetClaim(Claims.Subject), out var id)
            ? id
            : throw new InvalidOperationException("El token no tiene un usuario válido.");
}
