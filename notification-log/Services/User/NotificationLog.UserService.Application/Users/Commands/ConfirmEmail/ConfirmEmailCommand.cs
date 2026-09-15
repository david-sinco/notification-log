namespace NotificationLog.UserService.Application.Users.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Code);
