using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notifications.Application;
using Notifications.Application.Ports;
using Notifications.Domain;
using Notifications.Infrastructure.Messaging;
using Notifications.Infrastructure.Persistence;
using Notifications.Infrastructure.Sending;
using Shared.Messaging;

namespace Notifications.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddNotificationsInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(options =>
        {
            options.Connection(configuration.GetConnectionString("notificationsdb")
                ?? throw new InvalidOperationException("Missing connection string 'notificationsdb'."));

            // Dedup by caller-supplied idempotency key (SPEC.md §8) — Postgres enforces it, not
            // application-level locking.
            options.Schema.For<Notification>().UniqueIndex(n => n.IdempotencyKey);
        })
            // ContactReplicationService loads the existing UserContact to compare versions, then
            // stores a freshly-mapped instance for the same id. Marten's default identity-mapped
            // session refuses that second Store as a conflicting instance of an already-tracked
            // document — LightweightSession has no identity map, so this is a non-issue.
            .UseLightweightSessions();

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<IUserContactRepository, UserContactRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<NotificationCommandService>();
        services.AddScoped<ContactReplicationService>();

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

        services.AddSingleton<IMessageTransport>(_ => MessageTransportFactory.Create(
            configuration["Messaging:Kind"] ?? "RabbitMq",
            configuration["Messaging:ConnectionString"]
                ?? throw new InvalidOperationException("Missing configuration 'Messaging:ConnectionString'."),
            configuration.GetValue("Messaging:StreamPort", 5552)));

        services.AddHostedService<UsersContactConsumer>();
        services.AddHostedService<NotificationSenderWorker>();

        return services;
    }
}
