namespace NotificationLog.UserService.Application.Users.Commands.ConfirmPhone;

public sealed record ConfirmPhoneCommand(Guid UserId, string Code);
