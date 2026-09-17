namespace NotificationLog.IdentityService.Api.Accounts;

public sealed record AccountRegistration(
    string Identifier,
    string Password,
    string Name,
    string Locale,
    string TimeZone,
    bool AcceptsNotifications);
