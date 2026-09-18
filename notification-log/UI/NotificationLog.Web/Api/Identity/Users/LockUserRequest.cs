namespace NotificationLog.Web.Api.Identity.Users;

public sealed record LockUserRequest(DateTimeOffset? Until);
