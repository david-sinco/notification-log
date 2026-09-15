namespace NotificationLog.UserService.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Name, string Email, string? Phone);
