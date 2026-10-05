using NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Services.Sync;

public sealed class RecipientSyncService
{
    private readonly IRecipientRepository _recipients;
    private readonly CreateRecipientHandler _createRecipient;
    private readonly UpdateRecipientHandler _updateRecipient;

    public RecipientSyncService(
        IRecipientRepository recipients,
        CreateRecipientHandler createRecipient,
        UpdateRecipientHandler updateRecipient)
        => (_recipients, _createRecipient, _updateRecipient) = (recipients, createRecipient, updateRecipient);

    public async Task HandleAsync(CreateRecipientCommand command, CancellationToken ct)
    {
        if (!await _recipients.ExistsAsync(command.RecipientId, ct))
        {
            await _createRecipient.HandleAsync(command, ct);
            return;
        }

        await _updateRecipient.HandleAsync(
            new UpdateRecipientCommand(
                RecipientId: command.RecipientId,
                Name: command.Name,
                Email: command.Email,
                IsEmailVerified: !string.IsNullOrWhiteSpace(command.Email),
                Phone: command.Phone,
                IsPhoneVerified: !string.IsNullOrWhiteSpace(command.Phone),
                Locale: command.Locale,
                TimeZone: command.TimeZone,
                AcceptsNotifications: command.AcceptsNotifications),
            ct);
    }

    public Task HandleAsync(UpdateRecipientCommand command, CancellationToken ct)
        => _updateRecipient.HandleAsync(command, ct);
}
