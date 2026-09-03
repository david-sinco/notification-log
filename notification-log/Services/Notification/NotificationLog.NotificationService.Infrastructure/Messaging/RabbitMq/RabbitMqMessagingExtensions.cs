using System.Text.Json;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.Contracts.Users;
using Wolverine;
using Wolverine.Protobuf;
using Wolverine.RabbitMQ;
using Wolverine.Runtime.Serialization;

namespace NotificationLog.NotificationService.Infrastructure.Messaging.RabbitMq;

public static class RabbitMqMessagingExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("rabbitmq")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'rabbitmq'.");

        services.AddWolverine(opts =>
        {
            // RecipientSyncService depende de tipos internal (RecipientRepository,
            // CreateRecipientValidator) y de IUnitOfWork registrado por factory lambda — Wolverine
            // no puede "inlinear" esa cadena en el código generado y por defecto lo rechaza
            // (ServiceLocationPolicy.NotAllowed). Esos internal existen a propósito (encapsulan
            // detalles de Infrastructure/Application), así que se relaja la política en vez de
            // exponerlos.
            opts.ServiceLocationPolicy = ServiceLocationPolicy.AlwaysAllowed;

            opts.UseRabbitMq(new Uri(connectionString)).AutoProvision();

            // UseProtobufSerialization pone el serializador protobuf como DEFAULT de toda la app
            // (WolverineOptions.DefaultSerializer) — no hay un "RegisterSerializer" a nivel de
            // listener individual (RabbitMqListenerConfiguration no lo tiene). Para poder seguir
            // publicando JSON a mano desde la UI de RabbitMQ además del protobuf real, hay que sumar
            // un segundo serializador a nivel global con AddSerializer: Wolverine elige entre los
            // registrados según la propiedad AMQP "content_type" del mensaje (DetermineSerializer),
            // y si no la trae, cae en el default (protobuf). Content-type de cada uno:
            //   - protobuf (default, sin content_type): "binary/protobuf"
            //   - JSON (para pruebas manuales): "application/json"
            opts.UseProtobufSerialization();
            opts.AddSerializer(new SystemTextJsonSerializer(
                new JsonSerializerOptions(JsonSerializerDefaults.Web )));

            opts.ListenToRabbitQueue("user-changes")
                .DefaultIncomingMessage<UserContactUpdated>();
        });

        return services;
    }
}
