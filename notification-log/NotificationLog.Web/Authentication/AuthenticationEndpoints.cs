using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace NotificationLog.Web.Authentication;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthentication(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/authentication");

        group.MapGet("/login", (string? returnUrl) =>
                TypedResults.Challenge(
                    new AuthenticationProperties { RedirectUri = LocalUrl(returnUrl) },
                    [OpenIdConnectDefaults.AuthenticationScheme]))
            .AllowAnonymous();

        group.MapPost("/logout", ([FromForm] string? returnUrl) =>
            TypedResults.SignOut(
                new AuthenticationProperties { RedirectUri = LocalUrl(returnUrl) },
                [CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme]));

        group.MapGet("/access-denied", () =>
                TypedResults.Content("No tienes permiso para ver esta página.", "text/plain; charset=utf-8", statusCode: StatusCodes.Status403Forbidden))
            .AllowAnonymous();

        return app;
    }

    private static string LocalUrl(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && returnUrl.StartsWith('/') && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\")
            ? returnUrl
            : "/";
}
