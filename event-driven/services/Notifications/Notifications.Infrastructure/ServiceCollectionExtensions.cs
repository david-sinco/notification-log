using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Notifications.Application;
using Notifications.Application.Ports;
using Notifications.Infrastructure.EventLog;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Sending;
using Shared.EventLog;

namespace Notifications.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<IEventLog>(_ => new KafkaEventLog(
            configuration["Messaging:BootstrapServers"]
                ?? throw new InvalidOperationException("Missing configuration 'Messaging:BootstrapServers'.")));

        services.AddSingleton<ContactMaterializer>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<ContactMaterializer>());
        services.AddScoped<IUserContactRepository, InMemoryUserContactRepository>();

        // Singleton, not scoped: this repository's dictionaries ARE the storage — see its own
        // doc comment on why a fresh instance per request would silently lose data.
        services.AddSingleton<INotificationRepository, InMemoryNotificationRepository>();

        services.AddScoped<NotificationCommandService>();

        services.AddScoped<INotificationSender>(_ =>
        {
            var fake = new FakeNotificationSender();

            var smtpHost = configuration["Smtp:Host"];
            var smtpPort = configuration["Smtp:Port"];
            // Smtp:* is only set when AppHost wires up Mailpit; running standalone without it
            // falls back to the pure fake sender rather than failing to start.
            if (string.IsNullOrEmpty(smtpHost) || !int.TryParse(smtpPort, out var port))
                return fake;

            return new MailpitEmailSender(fake, smtpHost, port);
        });

        services.AddHostedService<NotificationSenderWorker>();

        return services;
    }
}
