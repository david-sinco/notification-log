using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.Contracts.Identity;
using NotificationLog.Contracts.Notifications;
using Wolverine;
using Wolverine.Protobuf;
using Wolverine.RabbitMQ;
using JasperFx.CodeGeneration.Model;

namespace NotificationLog.RentalService.Infrastructure.Messaging;

public static class RabbitMqMessagingExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'rabbitmq'.");

        services.AddWolverine(opts =>
        {
            opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;
            opts.Discovery.IncludeAssembly(typeof(RabbitMqMessagingExtensions).Assembly);
            opts.Policies.UseDurableLocalQueues();

            opts.UseRabbitMq(new Uri(connectionString)).AutoProvision();

            opts.ListenToRabbitQueue("rentals-person-verification")
                .DefaultIncomingMessage<PersonVerificationChanged>()
                .UseProtobufSerialization();

            opts.ListenToRabbitQueue("rentals-advisors")
                .DefaultIncomingMessage<AdvisorChanged>()
                .UseProtobufSerialization();

            opts.ListenToRabbitQueue("rentals-alerts-consents")
                .DefaultIncomingMessage<AlertsConsentChanged>()
                .UseProtobufSerialization();

            opts.PublishMessage<NotificationDispatchRequested>()
                .ToRabbitQueue("notification-dispatch")
                .UseProtobufSerialization();
        });

        return services;
    }
}
