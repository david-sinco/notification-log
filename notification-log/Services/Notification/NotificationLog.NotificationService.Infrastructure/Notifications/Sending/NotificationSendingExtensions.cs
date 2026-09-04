using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationLog.NotificationService.Application.Notifications.Services.Sending;
using NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Email;
using NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Push;
using NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Sms;
using NotificationLog.NotificationService.Infrastructure.Notifications.Sending.WhatsApp;

namespace NotificationLog.NotificationService.Infrastructure.Notifications.Sending;

public static class NotificationSendingExtensions
{
    // Cada canal tiene su propio objeto bajo "Notifications" en appsettings (Notifications:Email,
    // Notifications:Sms, Notifications:Push) — no un único bloque compartido — porque cada uno
    // apunta a un proveedor distinto con su propia forma de configurarse. WhatsApp no tiene
    // proveedor real: se registra un stub que siempre revienta, solo para que
    // NotificationDispatchService (que pide las cuatro interfaces en su constructor) se pueda
    // registrar en el contenedor.
    public static IServiceCollection AddNotificationSending(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailNotificationOptions>(configuration.GetSection(EmailNotificationOptions.SectionName));
        services.AddScoped<IEmailNotificationSender, SmtpEmailNotificationSender>();

        services.Configure<SmsNotificationOptions>(configuration.GetSection(SmsNotificationOptions.SectionName));
        services.AddHttpClient<ISmsNotificationSender, HttpSmsNotificationSender>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SmsNotificationOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.Configure<PushNotificationOptions>(configuration.GetSection(PushNotificationOptions.SectionName));
        services.AddHttpClient<IPushNotificationSender, HttpPushNotificationSender>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<PushNotificationOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        services.AddScoped<IWhatsAppNotificationSender, NotImplementedWhatsAppNotificationSender>();

        return services;
    }
}
