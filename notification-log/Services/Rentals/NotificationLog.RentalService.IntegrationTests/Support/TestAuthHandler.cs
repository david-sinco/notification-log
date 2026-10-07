using System.Security.Claims;
using System.Text.Encodings.Web;
using Domain.Shared.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string UserHeader = "X-Test-User";
    public const string RoleHeader = "X-Test-Role";
    public const string EmailHeader = "X-Test-Email";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(UserHeader, out var user))
            return Task.FromResult(AuthenticateResult.NoResult());

        List<Claim> claims =
        [
            new Claim("sub", user.ToString()),
            new Claim(ClaimTypes.Role, Request.Headers[RoleHeader].ToString()),
            new Claim(OpenIddictConstants.Claims.Private.Scope, OidcScopeNames.Rentals)
        ];

        if (Request.Headers.TryGetValue(EmailHeader, out var email))
            claims.Add(new Claim(OpenIddictConstants.Claims.Email, email.ToString()));

        var identity = new ClaimsIdentity(claims, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName)));
    }
}
