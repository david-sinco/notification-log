namespace NotificationLog.Web.Api.Identity.Users;

public sealed record UserDto(
    Guid Id,
    string Name,
    string? Email,
    bool IsEmailVerified,
    string? Phone,
    bool IsPhoneVerified,
    DateTimeOffset? LockedUntil,
    IReadOnlyList<string> Roles);
