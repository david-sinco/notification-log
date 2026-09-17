using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NotificationLog.NotificationService.Application.Notifications.Services.Sending;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Email;

// Contra Buggregator en dev (atrapa el SMTP, no lo entrega de verdad); contra un SMTP real en
// otros entornos, cambiando solo Notifications:Email en appsettings — el sender no sabe la
// diferencia.
public sealed class SmtpEmailNotificationSender : IEmailNotificationSender
{
    private readonly EmailNotificationOptions _options;

    public SmtpEmailNotificationSender(IOptions<EmailNotificationOptions> options)
        => _options = options.Value;

    public async Task<NotificationSendResult> SendAsync(
        string destination, RenderedMessage message, CancellationToken ct)
    {
        using var mail = new MailMessage(_options.From, destination)
        {
            Subject = message.Subject ?? string.Empty,
            Body = message.Body,
            // El cuerpo lo decide cada plantilla: las de texto plano seguirían llegando con las
            // etiquetas escapadas si esto fuera siempre true, y las HTML llegarían como código
            // fuente si fuera siempre false.
            IsBodyHtml = message.Body.TrimStart().StartsWith('<')
        };

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl
        };

        if (!string.IsNullOrEmpty(_options.Username))
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);

        try
        {
            await client.SendMailAsync(mail, ct);
            return NotificationSendResult.Success(null);
        }
        catch (SmtpException ex)
        {
            // Rechazo del servidor SMTP (destinatario inválido, etc.) — es un resultado de negocio,
            // no un error inesperado; fallas de transporte (conexión caída) sí se dejan propagar
            // para que NotificationDispatchService las registre como falla con su propio try/catch.
            return NotificationSendResult.Failure(ex.Message);
        }
    }
}
