namespace NotificationLog.IdentityService.Application.Users.Commands.LockUser;

public sealed record LockUserCommand(Guid Id, DateTimeOffset? Until);
