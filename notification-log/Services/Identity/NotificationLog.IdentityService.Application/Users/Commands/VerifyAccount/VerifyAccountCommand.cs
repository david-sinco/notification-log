namespace NotificationLog.IdentityService.Application.Users.Commands.VerifyAccount;

public sealed record VerifyAccountCommand(string Identifier, string Code);
