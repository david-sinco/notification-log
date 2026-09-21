namespace NotificationLog.IdentityService.Application.Users.Commands.SignInWithCode;

public sealed record SignInWithCodeCommand(string Identifier, string Code);
