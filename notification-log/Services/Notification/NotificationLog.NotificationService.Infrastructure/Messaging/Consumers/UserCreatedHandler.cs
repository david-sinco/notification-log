using NotificationLog.Contracts.Identity;
using NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;
using NotificationLog.NotificationService.Application.Recipients.Services.Sync;

namespace NotificationLog.NotificationService.Infrastructure.Messaging.Consumers;

public static class UserCreatedHandler
{
    public static Task Handle(UserCreated message, RecipientSyncService syncService, CancellationToken ct)
        => syncService.HandleAsync(
            new CreateRecipientCommand(
                RecipientId: Guid.Parse(message.UserId),
                Name: message.Name,
                Email: message.Email,
                Phone: message.Phone,
                Locale: message.Locale,
                TimeZone: message.TimeZone,
                AcceptsNotifications: message.AcceptsNotifications),
            ct);
}
