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
                Email: message.Email,
                Phone: message.Phone),
            ct);
}
