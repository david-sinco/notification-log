using Domain.Shared.Authorization;
using NotificationLog.Contracts.Identity;
using NotificationLog.IdentityService.Application.Users.Commands.CreateAccount;

namespace NotificationLog.IdentityService.Infrastructure.Messaging.Consumers;

public static class AccountCreationRequestedHandler
{
    public static async Task Handle(
        AccountCreationRequested message,
        CreateAccountHandler handler,
        CancellationToken ct)
        => await handler.HandleAsync(
            new CreateAccountCommand(
                Id: ParseId(message.UserId),
                Identifier: message.Email,
                Phone: message.Phone,
                Name: message.Name,
                Locale: message.Locale,
                TimeZone: message.TimeZone,
                AcceptsNotifications: message.AcceptsNotifications,
                Roles: [ParseRole(message.Role)]),
            ct);

    private static Guid ParseId(string userId) =>
        Guid.TryParse(userId, out var id)
            ? id
            : throw new ArgumentException($"El id de usuario '{userId}' no es un GUID válido.", nameof(userId));

    private static UserRole ParseRole(string role) =>
        Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsed) && Enum.IsDefined(parsed)
            ? parsed
            : throw new ArgumentException($"El rol '{role}' no existe.", nameof(role));
}
