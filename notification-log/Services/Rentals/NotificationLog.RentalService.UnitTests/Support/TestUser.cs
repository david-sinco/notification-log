using System.Security.Claims;
using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class TestUser
{
    public static ClaimsPrincipal With(UserRole role, Guid id)
        => new(new ClaimsIdentity([new Claim("sub", id.ToString()), new Claim(ClaimTypes.Role, role.ToString())], "tests"));
}
