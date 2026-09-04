using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Domain.Notifications;
using NotificationLog.NotificationService.Domain.Recipients;
using NotificationLog.NotificationService.Domain.Templates;
using NotificationLog.NotificationService.Domain.Triggers;
using NotificationLog.NotificationService.Infrastructure.Messaging.RabbitMq;
using NotificationLog.NotificationService.Application.Notifications.Services.Rendering;
using NotificationLog.NotificationService.Infrastructure.Notifications.Rendering;
using NotificationLog.NotificationService.Infrastructure.Notifications.Sending;
using NotificationLog.NotificationService.Infrastructure.Persistence.Context;
using NotificationLog.NotificationService.Infrastructure.Persistence.Repositories;

namespace NotificationLog.NotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Falta la cadena de conexión 'Default'.");

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
            }));

        services.AddScoped<IUnitOfWork>(sp =>
            sp.GetRequiredService<NotificationDbContext>());

        services.AddScoped<INotificationTriggerRepository, NotificationTriggerRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<IRecipientRepository, RecipientRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // Ejemplo de mensajería con RabbitMQ (Messaging/RabbitMq): consume UserChange y llama a
        // RecipientSyncService. Cuando se integre Kafka, este llamado se reemplaza por el
        // equivalente en Messaging/Kafka sin tocar Application.
        services.AddRabbitMqMessaging(configuration);

        // Remitentes por canal (Notifications/Sending): Email, SMS y Push, cada uno configurado
        // desde su propio objeto bajo "Notifications" en appsettings. WhatsApp queda sin
        // implementación — IWhatsAppNotificationSender sigue sin registrar.
        services.AddNotificationSending(configuration);

        // Sin estado ni configuración externa (a diferencia de los senders): Scriban parsea y
        // renderiza en memoria, así que un singleton alcanza.
        services.AddSingleton<ITemplateRenderer, ScribanTemplateRenderer>();

        return services;
    }
}