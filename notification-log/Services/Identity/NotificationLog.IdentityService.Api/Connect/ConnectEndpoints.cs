using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using NotificationLog.IdentityService.Api.Accounts;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace NotificationLog.IdentityService.Api.Connect;

public static class ConnectEndpoints
{
    public static IEndpointRouteBuilder MapConnect(this IEndpointRouteBuilder app)
    {
        app.MapMethods("/connect/authorize", [HttpMethods.Get, HttpMethods.Post], AuthorizeAsync)
            .ExcludeFromDescription();

        app.MapPost("/connect/token", (Delegate)ExchangeAsync)
            .ExcludeFromDescription();

        app.MapMethods("/connect/endsession", [HttpMethods.Get, HttpMethods.Post], (Delegate)EndSessionAsync)
            .ExcludeFromDescription();

        return app;
    }

    private static async Task<IResult> AuthorizeAsync(
        HttpContext context,
        AccountService accounts,
        IOpenIddictScopeManager scopes,
        CancellationToken ct)
    {
        var request = context.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException("La solicitud OpenID Connect no es válida.");

        var session = await context.AuthenticateAsync(SessionCookie.Scheme);
        var user = session.Succeeded ? await ValidateAsync(session.Principal, accounts, ct) : null;

        if (user is null)
        {
            if (session.Succeeded)
                await context.SignOutAsync(SessionCookie.Scheme);

            if (request.HasPromptValue(PromptValues.None))
                return Forbid(Errors.LoginRequired, "El usuario no ha iniciado sesión.");

            var returnUrl = context.Request.PathBase + context.Request.Path + QueryString.Create(
                context.Request.HasFormContentType ? context.Request.Form : context.Request.Query);

            return Results.Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, [SessionCookie.Scheme]);
        }

        var requestedScopes = request.GetScopes();
        var resources = await scopes.ListResourcesAsync(requestedScopes, ct).ToListAsync(ct);

        return Results.SignIn(
            OidcPrincipalFactory.CreateToken(user, requestedScopes, resources),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> ExchangeAsync(
        HttpContext context,
        AccountService accounts,
        CancellationToken ct)
    {
        var principal = (await context.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)).Principal;
        var user = principal is null ? null : await ValidateAsync(principal, accounts, ct);

        if (principal is null || user is null)
            return Forbid(Errors.InvalidGrant, "La sesión ya no es válida.");

        return Results.SignIn(
            OidcPrincipalFactory.CreateToken(user, principal.GetScopes(), principal.GetResources()),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> EndSessionAsync(HttpContext context)
    {
        await context.SignOutAsync(SessionCookie.Scheme);

        return Results.SignOut(
            new AuthenticationProperties { RedirectUri = "/" },
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static async Task<SignedInUser?> ValidateAsync(ClaimsPrincipal principal, AccountService accounts, CancellationToken ct)
    {
        var stamp = principal.GetClaim(OidcClaims.SecurityStamp);

        if (!Guid.TryParse(principal.GetClaim(Claims.Subject), out var userId) || string.IsNullOrEmpty(stamp))
            return null;

        return await accounts.ValidateSessionAsync(userId, stamp, ct);
    }

    private static IResult Forbid(string error, string description) =>
        Results.Forbid(
            new AuthenticationProperties(new Dictionary<string, string?>
            {
                [OpenIddictServerAspNetCoreConstants.Properties.Error] = error,
                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = description
            }),
            [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
}
