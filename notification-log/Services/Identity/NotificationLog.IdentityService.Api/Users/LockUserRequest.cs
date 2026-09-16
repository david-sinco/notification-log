namespace NotificationLog.IdentityService.Api.Users;

public sealed record LockUserRequest(DateTimeOffset? Until);
