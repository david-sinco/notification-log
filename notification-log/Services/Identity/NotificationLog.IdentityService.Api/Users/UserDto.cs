namespace NotificationLog.IdentityService.Api.Users;

public sealed record UserDto(
    Guid Id,
    string? Email,
    bool IsEmailVerified,
    string? Phone,
    bool IsPhoneVerified,
    DateTimeOffset? LockedUntil,
    IReadOnlyList<string> Roles);
