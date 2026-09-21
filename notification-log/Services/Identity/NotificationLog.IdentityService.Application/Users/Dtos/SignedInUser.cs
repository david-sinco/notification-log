using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Dtos;

public sealed record SignedInUser(
    Guid Id,
    string? Email,
    string? Phone,
    string SecurityStamp,
    IReadOnlyList<string> Roles)
{
    public string DisplayName => Email ?? Phone ?? Id.ToString();

    public static SignedInUser From(User user) =>
        new(user.Id, user.Email, user.Phone, user.SecurityStamp, user.Roles);
}
