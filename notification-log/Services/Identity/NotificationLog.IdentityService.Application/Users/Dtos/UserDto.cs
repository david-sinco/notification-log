using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Application.Users.Dtos;

public sealed record UserDto(
    Guid Id,
    string? Email,
    bool IsEmailVerified,
    string? Phone,
    bool IsPhoneVerified,
    DateTimeOffset? LockedUntil,
    IReadOnlyList<string> Roles)
{
    public static UserDto From(User user, DateTimeOffset now) =>
        new(user.Id,
            user.Email,
            user.IsEmailConfirmed,
            user.Phone,
            user.IsPhoneConfirmed,
            user.IsLockedOut(now) ? user.LockedUntil : null,
            user.Roles);
}
