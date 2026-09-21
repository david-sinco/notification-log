namespace NotificationLog.IdentityService.Api.Contracts.Users;

public sealed record LockUserRequest(DateTimeOffset? Until);
