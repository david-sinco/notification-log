using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using NotificationLog.Web.Api;
using NotificationLog.Web.Authentication;

namespace NotificationLog.Web.Components.Rentals;

public sealed class RentalsActor
{
    private readonly AuthenticationStateProvider _authentication;

    public RentalsActor(AuthenticationStateProvider authentication) => _authentication = authentication;

    public Guid? Id => Guid.TryParse(User.FindFirstValue(IdentityClaims.Subject), out var id) ? id : null;

    public bool IsStaff => User.IsInRole(IdentityRoles.Administrador) || User.IsInRole(IdentityRoles.Moderador);

    public Guid Require() =>
        Id ?? throw new ApiException(
            HttpStatusCode.Unauthorized,
            "Inicia sesión",
            "Necesitas iniciar sesión para hacer esto.");

    public bool Is(Guid id) => Id == id;

    private ClaimsPrincipal User
    {
        get
        {
            var state = _authentication.GetAuthenticationStateAsync();

            return state.IsCompletedSuccessfully ? state.Result.User : new ClaimsPrincipal();
        }
    }
}
