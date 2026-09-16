using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace NotificationLog.IdentityService.Api.Accounts.Verification;

public sealed class VerificationCodeSender
{
    private readonly HttpClient _http;
    private readonly SmtpOptions _smtp;
    private readonly SmsOptions _sms;

    public VerificationCodeSender(HttpClient http, IOptions<SmtpOptions> smtp, IOptions<SmsOptions> sms)
        => (_http, _smtp, _sms) = (http, smtp.Value, sms.Value);

    public Task SendAsync(LoginIdentifier destination, string code, CancellationToken ct)
    {
        var message = $"Tu código para confirmar la cuenta es {code}. Vence en pocos minutos.";

        return destination.Channel == LoginChannel.Email
            ? SendEmailAsync(destination.Value, message, ct)
            : SendSmsAsync(destination.Value, message, ct);
    }

    private async Task SendEmailAsync(string email, string message, CancellationToken ct)
    {
        using var mail = new MailMessage(_smtp.From, email)
        {
            Subject = "Confirma tu cuenta",
            Body = message
        };

        using var client = new SmtpClient(_smtp.Host, _smtp.Port) { EnableSsl = _smtp.EnableSsl };

        if (!string.IsNullOrEmpty(_smtp.Username))
            client.Credentials = new NetworkCredential(_smtp.Username, _smtp.Password);

        await client.SendMailAsync(mail, ct);
    }

    private async Task SendSmsAsync(string phone, string message, CancellationToken ct)
    {
        var response = await _http.PostAsJsonAsync(
            new Uri(new Uri(_sms.BaseUrl), _sms.Path),
            new { from = _sms.From, to = phone, message },
            ct);

        response.EnsureSuccessStatusCode();
    }
}
