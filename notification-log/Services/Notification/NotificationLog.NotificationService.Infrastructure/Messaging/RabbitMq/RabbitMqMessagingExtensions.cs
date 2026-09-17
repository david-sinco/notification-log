using System.Text.Json;
using System.Text.Json.Serialization;
using JasperFx.CodeGeneration.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.Contracts.Notifications;
using Wolverine;
using Wolverine.Protobuf;
using Wolverine.RabbitMQ;
using Wolverine.Runtime.Serialization;

namespace NotificationLog.NotificationService.Infrastructure.Messaging.RabbitMq;

public static class RabbitMqMessagingExtensions
{
    private const string IdentityUsersExchange = "identity.users";
    private const string IdentityUsersQueue = "notification-identity-users";
    private const string VerificationCodesExchange = "identity.verification-codes";
    private const string VerificationCodesQueue = "notification-verification-codes";

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

            opts.UseRabbitMq(new Uri(connectionString))
                .DeclareExchange(IdentityUsersExchange, exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Fanout;
                    exchange.BindQueue(IdentityUsersQueue);
                })
                .DeclareExchange(VerificationCodesExchange, exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Fanout;
                    exchange.BindQueue(VerificationCodesQueue);
                })
                .AutoProvision();

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
            // PreferredObjectCreationHandling = Populate no es un detalle de estilo: los campos
            // "map" de protobuf (data en NotificationDispatchRequested) se generan como propiedades SOLO LECTURA
            // (MapField<K,V> sin setter, inicializada en el constructor). System.Text.Json ignora
            // en silencio toda propiedad sin setter al deserializar, así que sin esto un JSON
            // publicado a mano llegaba con el map VACÍO y sin ningún error: el render con
            // StrictVariables fallaba después, lejos de la causa. Con Populate, STJ escribe sobre
            // la instancia que el mensaje ya trae en vez de intentar reemplazarla.
            opts.AddSerializer(new SystemTextJsonSerializer(
                new JsonSerializerOptions(JsonSerializerDefaults.Web)
                {
                    PreferredObjectCreationHandling = JsonObjectCreationHandling.Populate
                }));

            opts.ListenToRabbitQueue(IdentityUsersQueue);

            // Exchange propio y no el fanout identity.users: el código de verificación viaja en el
            // mensaje, así que solo debe llegar a esta cola y no a los demás consumidores de
            // Identity (Rentals está enlazado a identity.users).
            opts.ListenToRabbitQueue(VerificationCodesQueue);

            // notification-dispatch: cola donde cualquier servicio publica "esto pasó"
            // (NotificationDispatchRequested) — la publicación todavía se hace a mano (UI de
            // administración de RabbitMQ) mientras no exista un productor real. La consume
            // NotificationDispatchHandler (Messaging/Consumers), que llama a
            // NotificationDispatchService.
            opts.ListenToRabbitQueue("notification-dispatch")
                .DefaultIncomingMessage<NotificationDispatchRequested>();
        });

        return services;
    }
}
