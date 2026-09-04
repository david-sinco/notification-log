using Microsoft.Extensions.Logging;
using NotificationLog.NotificationService.Application.Notifications.Commands.RecordNotificationFailure;
using NotificationLog.NotificationService.Application.Notifications.Commands.RecordNotificationSuccess;
using NotificationLog.NotificationService.Application.Notifications.Services.Rendering;
using NotificationLog.NotificationService.Application.Notifications.Services.Sending;
using NotificationLog.NotificationService.Domain.Notifications;
using NotificationLog.NotificationService.Domain.Recipients;
using NotificationLog.NotificationService.Domain.Shared;
using NotificationLog.NotificationService.Domain.Templates;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Notifications.Services.Dispatch;

// Puerto que invoca el consumer de infraestructura (p. ej. un IConsumer<T> de MassTransit) cada
// vez que llega un evento de negocio. No hace fetch: el mensaje se lo empujan una vez por invocación,
// así que esta clase se resuelve en el scope que ya crea el propio consumer, sin loop ni polling.
public sealed class NotificationDispatchService
{
    private readonly INotificationTriggerRepository _triggers;
    private readonly IRecipientRepository _recipients;
    private readonly INotificationTemplateRepository _templates;
    private readonly ITemplateRenderer _renderer;
    private readonly IEmailNotificationSender _emailSender;
    private readonly ISmsNotificationSender _smsSender;
    private readonly IPushNotificationSender _pushSender;
    private readonly IWhatsAppNotificationSender _whatsAppSender;
    private readonly RecordNotificationSuccessHandler _recordSuccess;
    private readonly RecordNotificationFailureHandler _recordFailure;
    private readonly ILogger<NotificationDispatchService> _log;

    public NotificationDispatchService(
        INotificationTriggerRepository triggers,
        IRecipientRepository recipients,
        INotificationTemplateRepository templates,
        ITemplateRenderer renderer,
        IEmailNotificationSender emailSender,
        ISmsNotificationSender smsSender,
        IPushNotificationSender pushSender,
        IWhatsAppNotificationSender whatsAppSender,
        RecordNotificationSuccessHandler recordSuccess,
        RecordNotificationFailureHandler recordFailure,
        ILogger<NotificationDispatchService> log)
        => (_triggers, _recipients, _templates, _renderer, _emailSender, _smsSender, _pushSender,
            _whatsAppSender, _recordSuccess, _recordFailure, _log)
            = (triggers, recipients, templates, renderer, emailSender, smsSender, pushSender,
               whatsAppSender, recordSuccess, recordFailure, log);

    public async Task DispatchAsync(BusinessEvent businessEvent, CancellationToken ct)
    {
        try
        {
            var trigger = await _triggers.GetByEventKeyAsync(EventKey.Create(businessEvent.EventKey), ct);

            if (trigger is null || !trigger.IsEnabled)
            {
                _log.LogWarning(
                    "No hay un trigger habilitado para el evento {EventKey}", businessEvent.EventKey);
                return;
            }

            var recipient = await _recipients.GetByIdAsync(businessEvent.RecipientId, ct);

            if (recipient is null)
            {
                _log.LogWarning("El destinatario {RecipientId} no existe", businessEvent.RecipientId);
                return;
            }

            foreach (var configuration in trigger.ResolveActiveConfigurations())
                await DispatchConfigurationAsync(businessEvent, trigger.EventKey.Value, configuration, recipient, ct);
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "Fallo inesperado al procesar el evento {EventId} ({EventKey}) para el destinatario {RecipientId}",
                businessEvent.EventId, businessEvent.EventKey, businessEvent.RecipientId);
            throw;
        }
    }

    private async Task DispatchConfigurationAsync(
        BusinessEvent businessEvent,
        string eventKey,
        NotificationConfiguration configuration,
        Recipient recipient,
        CancellationToken ct)
    {
        if (!recipient.CanReceive(configuration.Channel))
        {
            _log.LogWarning(
                "El destinatario {RecipientId} no puede recibir por el canal {Channel}",
                recipient.Id, configuration.Channel);
            return;
        }

        var template = await _templates.GetByIdAsync(configuration.TemplateId, ct);

        if (template is null)
        {
            _log.LogWarning("La plantilla {TemplateId} no existe", configuration.TemplateId);
            return;
        }

        var destination = recipient.AddressFor(configuration.Channel)!;
        var version = template.CurrentVersion;

        RenderedMessage message;
        try
        {
            var data = _renderer.BuildData(businessEvent, recipient);
            message = _renderer.Render(version.Subject, version.Body, data);
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "Fallo al renderizar la plantilla {TemplateId} para el evento {EventKey}",
                template.Id, eventKey);

            await _recordFailure.HandleAsync(
                new RecordNotificationFailureCommand(
                    businessEvent.EventId, eventKey, configuration.Id, template.Id, version.Id, recipient.Id,
                    configuration.Channel, destination, ex.Message, DateTime.UtcNow, businessEvent.Data),
                ct);
            return;
        }

        try
        {
            var result = await SendAsync(configuration.Channel, destination, message, ct);

            if (result.Succeeded)
            {
                await _recordSuccess.HandleAsync(
                    new RecordNotificationSuccessCommand(
                        businessEvent.EventId, eventKey, configuration.Id, template.Id, version.Id, recipient.Id,
                        configuration.Channel, destination, result.ProviderMessageId, DateTime.UtcNow,
                        businessEvent.Data),
                    ct);
            }
            else
            {
                await _recordFailure.HandleAsync(
                    new RecordNotificationFailureCommand(
                        businessEvent.EventId, eventKey, configuration.Id, template.Id, version.Id, recipient.Id,
                        configuration.Channel, destination, result.Error ?? "Error desconocido", DateTime.UtcNow,
                        businessEvent.Data),
                    ct);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex,
                "Fallo al enviar la notificación del evento {EventKey} al destinatario {RecipientId} por {Channel}",
                eventKey, recipient.Id, configuration.Channel);

            await _recordFailure.HandleAsync(
                new RecordNotificationFailureCommand(
                    businessEvent.EventId, eventKey, configuration.Id, template.Id, version.Id, recipient.Id,
                    configuration.Channel, destination, ex.Message, DateTime.UtcNow, businessEvent.Data),
                ct);
        }
    }

    private Task<NotificationSendResult> SendAsync(
        NotificationChannel channel, string destination, RenderedMessage message, CancellationToken ct)
        => channel switch
        {
            NotificationChannel.Email => _emailSender.SendAsync(destination, message, ct),
            NotificationChannel.Sms => _smsSender.SendAsync(destination, message, ct),
            NotificationChannel.Push => _pushSender.SendAsync(destination, message, ct),
            NotificationChannel.WhatsApp => _whatsAppSender.SendAsync(destination, message, ct),
            _ => throw new InvalidOperationException($"El canal '{channel}' no tiene un remitente registrado.")
        };
}
