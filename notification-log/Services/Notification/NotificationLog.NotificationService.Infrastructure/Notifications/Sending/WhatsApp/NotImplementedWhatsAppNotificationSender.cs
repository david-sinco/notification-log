using NotificationLog.NotificationService.Application.Notifications.Services.Sending;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Infrastructure.Notifications.Sending.WhatsApp;

// Sin proveedor de WhatsApp elegido todavía. Este stub existe únicamente para que
// NotificationDispatchService pueda registrarse en el contenedor — su constructor pide las cuatro
// interfaces de envío — y revienta siempre a propósito: un mensaje de WhatsApp que "se envía" en
// silencio sería peor que uno que queda registrado como notificación fallida con el motivo exacto.
public sealed class NotImplementedWhatsAppNotificationSender : IWhatsAppNotificationSender
{
    public Task<NotificationSendResult> SendAsync(string destination, RenderedMessage message, CancellationToken ct)
        => throw new NotSupportedException("IWhatsAppNotificationSender todavía no tiene una implementación real.");
}
