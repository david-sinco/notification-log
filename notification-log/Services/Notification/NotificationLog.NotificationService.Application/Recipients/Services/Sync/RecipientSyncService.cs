using Microsoft.Extensions.Logging;
using NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;
using NotificationLog.NotificationService.Application.Recipients.Commands.SetRecipientStatus;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientAttributes;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientEmail;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientPhone;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientProfile;

namespace NotificationLog.NotificationService.Application.Recipients.Services.Sync;

// Puerto que invoca el consumer de infraestructura (p. ej. un IConsumer<T> de MassTransit) cada
// vez que llega un cambio de usuario. No hace fetch: el mensaje se lo empujan una vez por
// invocación, así que esta clase se resuelve en el scope que ya crea el propio consumer, sin
// loop ni polling.
public sealed class RecipientSyncService
{
    private readonly CreateRecipientHandler _createRecipient;
    private readonly UpdateRecipientEmailHandler _updateEmail;
    private readonly UpdateRecipientPhoneHandler _updatePhone;
    private readonly UpdateRecipientProfileHandler _updateProfile;
    private readonly UpdateRecipientAttributesHandler _updateAttributes;
    private readonly SetRecipientStatusHandler _setStatus;
    private readonly ILogger<RecipientSyncService> _log;

    public RecipientSyncService(
        CreateRecipientHandler createRecipient,
        UpdateRecipientEmailHandler updateEmail,
        UpdateRecipientPhoneHandler updatePhone,
        UpdateRecipientProfileHandler updateProfile,
        UpdateRecipientAttributesHandler updateAttributes,
        SetRecipientStatusHandler setStatus,
        ILogger<RecipientSyncService> log)
        => (_createRecipient, _updateEmail, _updatePhone, _updateProfile, _updateAttributes, _setStatus, _log)
            = (createRecipient, updateEmail, updatePhone, updateProfile, updateAttributes, setStatus, log);

    public async Task HandleAsync(UserChange change, CancellationToken ct)
    {
        switch (change.Kind)
        {
            case UserChangeKind.Created:
                await _createRecipient.HandleAsync(
                    new CreateRecipientCommand(
                        change.UserId, change.Name ?? "", change.Email, change.Phone), ct);
                break;

            case UserChangeKind.EmailUpdated:
                await _updateEmail.HandleAsync(new UpdateRecipientEmailCommand(change.UserId, change.Email), ct);
                break;

            case UserChangeKind.PhoneUpdated:
                await _updatePhone.HandleAsync(new UpdateRecipientPhoneCommand(change.UserId, change.Phone), ct);
                break;

            case UserChangeKind.ProfileUpdated:
                await _updateProfile.HandleAsync(
                    new UpdateRecipientProfileCommand(
                        change.UserId, change.Name ?? "", change.Locale, change.TimeZone), ct);
                break;

            case UserChangeKind.AttributesUpdated:
                await _updateAttributes.HandleAsync(
                    new UpdateRecipientAttributesCommand(
                        change.UserId, change.Attributes ?? new Dictionary<string, string?>()), ct);
                break;

            case UserChangeKind.StatusUpdated:
                await _setStatus.HandleAsync(
                    new SetRecipientStatusCommand(change.UserId, change.IsActive ?? true), ct);
                break;

            default:
                _log.LogWarning("Tipo de cambio no soportado: {Kind}", change.Kind);
                break;
        }
    }
}
