using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Dtos;

public sealed record SignedInUser(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string SecurityStamp,
    string? Role)
{
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Email ?? Phone ?? Id.ToString() : Name;

    public static SignedInUser From(User user) =>
        new(
            user.Id,
            user.Name,
            NullIfEmpty(user.VerifiedEmail),
            NullIfEmpty(user.VerifiedPhone),
            user.SecurityStamp,
            user.Role?.ToString());

    private static string? NullIfEmpty(string value) => string.IsNullOrEmpty(value) ? null : value;
}
