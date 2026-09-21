namespace NotificationLog.IdentityService.Application.Users.Commands.RegisterAccount;

public sealed record RegisterAccountCommand(
    string Identifier,
    string Password,
    string Name,
    string Locale,
    string TimeZone,
    bool AcceptsNotifications);
