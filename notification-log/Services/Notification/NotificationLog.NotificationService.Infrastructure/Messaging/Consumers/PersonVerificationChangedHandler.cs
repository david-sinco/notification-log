using NotificationLog.Contracts.Identity;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;
using NotificationLog.NotificationService.Application.Recipients.Services.Sync;

namespace NotificationLog.NotificationService.Infrastructure.Messaging.Consumers;

public static class PersonVerificationChangedHandler
{
    public static Task Handle(PersonVerificationChanged message, RecipientSyncService syncService, CancellationToken ct)
        => syncService.HandleAsync(
            new UpdateRecipientCommand(
                RecipientId: Guid.Parse(message.UserId),
                Name: null,
                Email: message.Email,
                IsEmailVerified: !string.IsNullOrWhiteSpace(message.Email),
                Phone: message.Phone,
                IsPhoneVerified: !string.IsNullOrWhiteSpace(message.Phone),
                Locale: null,
                TimeZone: null,
                AcceptsNotifications: null),
            ct);
}
