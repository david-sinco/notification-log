namespace NotificationLog.UserService.Application.Users.Commands.SetUserStatus;

public sealed record SetUserStatusCommand(Guid UserId, bool IsActive, string? Reason);
