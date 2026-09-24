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
    private const string IdentityUsersExchange = "identity.users";
    private const string IdentityUsersQueue = "rentals-identity-users";

    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'rabbitmq'.");

        services.AddWolverine(opts =>
        {
            opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;
            opts.Discovery.IncludeAssembly(typeof(RabbitMqMessagingExtensions).Assembly);
            opts.Policies.UseDurableLocalQueues();

            opts.UseRabbitMq(new Uri(connectionString))
                .DeclareExchange(IdentityUsersExchange, exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Fanout;
                    exchange.BindQueue(IdentityUsersQueue);
                })
                .AutoProvision();

            opts.ListenToRabbitQueue(IdentityUsersQueue)
                .UseProtobufSerialization();

            opts.PublishMessage<NotificationDispatchRequested>()
                .ToRabbitQueue("notification-dispatch")
                .UseProtobufSerialization();

            opts.PublishMessage<AccountCreationRequested>()
                .ToRabbitQueue("identity-accounts")
                .UseProtobufSerialization();
        });

        return services;
    }
}
