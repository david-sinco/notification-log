using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;
using NotificationLog.NotificationService.Application.Recipients.Commands.SetRecipientStatus;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientAttributes;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientEmail;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientPhone;
using NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientProfile;

namespace NotificationLog.NotificationService.Application.Recipients.Sync;

public sealed class RecipientSyncService
{
    private readonly IUserChangeStream _stream;
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<RecipientSyncService> _log;

    public RecipientSyncService(
        IUserChangeStream stream,
        IServiceScopeFactory scopes,
        ILogger<RecipientSyncService> log)
        => (_stream, _scopes, _log) = (stream, scopes, log);

    public async Task RunAsync(CancellationToken ct)
    {
        await foreach (var change in _stream.ReadAsync(ct))
        {
            try
            {
                await DispatchAsync(change, ct);
            }
            catch (Exception ex)
            {
                _log.LogError(ex,
                    "Fallo al aplicar el cambio {Kind} del usuario {UserId}",
                    change.Kind, change.UserId);
            }
        }
    }

    private async Task DispatchAsync(UserChange change, CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var sp = scope.ServiceProvider;

        switch (change.Kind)
        {
            case UserChangeKind.Created:
                await sp.GetRequiredService<CreateRecipientHandler>().HandleAsync(
                    new CreateRecipientCommand(
                        change.UserId, change.Name ?? "", change.Email, change.Phone), ct);
                break;

            case UserChangeKind.EmailUpdated:
                await sp.GetRequiredService<UpdateRecipientEmailHandler>().HandleAsync(
                    new UpdateRecipientEmailCommand(change.UserId, change.Email), ct);
                break;

            case UserChangeKind.PhoneUpdated:
                await sp.GetRequiredService<UpdateRecipientPhoneHandler>().HandleAsync(
                    new UpdateRecipientPhoneCommand(change.UserId, change.Phone), ct);
                break;

            case UserChangeKind.ProfileUpdated:
                await sp.GetRequiredService<UpdateRecipientProfileHandler>().HandleAsync(
                    new UpdateRecipientProfileCommand(
                        change.UserId, change.Name ?? "", change.Locale, change.TimeZone), ct);
                break;

            case UserChangeKind.AttributesUpdated:
                await sp.GetRequiredService<UpdateRecipientAttributesHandler>().HandleAsync(
                    new UpdateRecipientAttributesCommand(
                        change.UserId, change.Attributes ?? new Dictionary<string, string?>()), ct);
                break;

            case UserChangeKind.StatusUpdated:
                await sp.GetRequiredService<SetRecipientStatusHandler>().HandleAsync(
                    new SetRecipientStatusCommand(change.UserId, change.IsActive ?? true), ct);
                break;

            default:
                _log.LogWarning("Tipo de cambio no soportado: {Kind}", change.Kind);
                break;
        }
    }
}