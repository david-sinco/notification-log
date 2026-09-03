using NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Services.Sync;

// Puerto que invoca el consumer de infraestructura cada vez que llega un cambio de usuario. No
// hace fetch: el mensaje se lo empujan una vez por invocación, así que esta clase se resuelve en
// el scope que ya crea el propio consumer, sin loop ni polling.
//
// Solo dos comandos, no uno por campo: el mensaje llega de un único punto y ya representa el
// estado final completo del usuario (event-carried state transfer), así que no hay motivo para
// descomponerlo en varios comandos independientes, cada uno con su propio SaveChangesAsync — eso
// dejaba la sincronización sin una transacción global (si el tercer comando fallaba, los dos
// anteriores ya habían comiteado). Con UpdateRecipientCommand aplicando todos los campos sobre la
// misma instancia cargada una vez, la atomicidad sale gratis: es un solo SaveChangesAsync.
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

    public async Task HandleAsync(UserChange change, CancellationToken ct)
    {
        var exists = await _recipients.ExistsAsync(change.UserId, ct);

        if (!exists)
        {
            var createCommand = new CreateRecipientCommand(
                change.UserId, change.Name ?? "", change.Email, change.Phone,
                change.Locale, change.TimeZone, change.IsActive, change.Attributes);
            await _createRecipient.HandleAsync(createCommand, ct);
            return;
        }

        var updateCommand = new UpdateRecipientCommand(
            change.UserId, change.Name, change.Email, change.Phone,
            change.Locale, change.TimeZone, change.IsActive, change.Attributes);

        await _updateRecipient.HandleAsync(updateCommand, ct);
    }
}
