using System.Security.Claims;
using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class TestUser
{
    public static ClaimsPrincipal With(UserRole role, Guid id, string? email = null)
    {
        List<Claim> claims = [new Claim("sub", id.ToString()), new Claim(ClaimTypes.Role, role.ToString())];

        if (email is not null)
            claims.Add(new Claim("email", email));

        return new(new ClaimsIdentity(claims, "tests"));
    }
}
