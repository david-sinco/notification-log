using System.Net.Mail;
using Notifications.Application.Ports;
using Notifications.Domain;

namespace Notifications.Infrastructure.Sending;

/// <summary>
/// Decorates the fake sender's deterministic pass/fail decision (SPEC.md §8) with a real SMTP
/// delivery to Mailpit whenever that decision is "Sent" for the Email channel, so a successful
/// send can actually be opened and read instead of only trusted on faith. SMS has no equivalent
/// local capture tool, so it stays purely fake. If Mailpit itself is unreachable, that counts as
/// a transient failure — the notification retries rather than being silently marked sent.
/// </summary>
public sealed class MailpitEmailSender(INotificationSender inner, string smtpHost, int smtpPort) : INotificationSender
{
    public async Task<SendResult> SendAsync(Notification notification, UserContact contact, CancellationToken ct)
    {
        var result = await inner.SendAsync(notification, contact, ct);
        if (result.Outcome != SendOutcome.Sent || notification.Channel != NotificationChannel.Email)
            return result;

        try
        {
            using var client = new SmtpClient(smtpHost, smtpPort);
            using var message = new MailMessage(
                from: "notifications@eventsourcinglab.local",
                to: contact.Email!, // CanReceive(Email) already guarantees this is set
                subject: "Event Sourcing Lab notification",
                body: notification.Body);

            await client.SendMailAsync(message, ct);
            return result;
        }
        catch (Exception ex)
        {
            return new SendResult(SendOutcome.TransientFailure, $"Mailpit delivery failed: {ex.Message}");
        }
    }
}
