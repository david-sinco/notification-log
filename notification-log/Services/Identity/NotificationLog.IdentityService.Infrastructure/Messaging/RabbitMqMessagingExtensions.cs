using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.Contracts.Identity;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.Protobuf;
using Wolverine.RabbitMQ;

namespace NotificationLog.IdentityService.Infrastructure.Messaging;

public static class RabbitMqMessagingExtensions
{
    public const string UsersExchange = "identity.users";
    public const string VerificationCodesExchange = "identity.verification-codes";
    public const string WolverineSchema = "wolverine";

    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        var identity = configuration.GetConnectionString("identity-db")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'identity-db'.");

        var rabbitmq = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'rabbitmq'.");

        services.AddWolverine(opts =>
        {
            opts.PersistMessagesWithPostgresql(identity, WolverineSchema);
            opts.UseEntityFrameworkCoreTransactions();
            opts.Policies.UseDurableOutboxOnAllSendingEndpoints();

            opts.UseRabbitMq(new Uri(rabbitmq)).AutoProvision();

            opts.PublishMessage<UserCreated>()
                .ToRabbitExchange(UsersExchange, exchange => exchange.ExchangeType = ExchangeType.Fanout)
                .UseProtobufSerialization();

            opts.PublishMessage<PersonVerificationChanged>()
                .ToRabbitExchange(UsersExchange, exchange => exchange.ExchangeType = ExchangeType.Fanout)
                .UseProtobufSerialization();

            opts.PublishMessage<VerificationCodeRequested>()
                .ToRabbitExchange(VerificationCodesExchange, exchange => exchange.ExchangeType = ExchangeType.Fanout)
                .UseProtobufSerialization();
        });

        return services;
    }
}
