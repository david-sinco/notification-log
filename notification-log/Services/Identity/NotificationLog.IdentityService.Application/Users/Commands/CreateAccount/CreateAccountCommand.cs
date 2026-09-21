using Domain.Shared.Authorization;

namespace NotificationLog.IdentityService.Application.Users.Commands.CreateAccount;

public sealed record CreateAccountCommand(
    Guid Id,
    string Identifier,
    string Phone,
    string Name,
    string Locale,
    string TimeZone,
    bool AcceptsNotifications,
    IReadOnlyList<UserRole> Roles);
