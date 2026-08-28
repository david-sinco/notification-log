namespace Shared.Messaging;

public static class MessageTransportFactory
{
    /// <summary>
    /// Reads Messaging:Kind ("RabbitMq" | "RabbitMqStream" | "Kafka") and
    /// Messaging:ConnectionString. streamPort is only consulted for "RabbitMqStream" — it's a
    /// separate listener on the same broker connectionString already points at (SPEC.md §7).
    /// </summary>
    public static IMessageTransport Create(string kind, string connectionString, int streamPort = 5552) => kind switch
    {
        "RabbitMq" => new RabbitMqTransport(connectionString),
        "RabbitMqStream" => new RabbitMqStreamTransport(connectionString, streamPort),
        "Kafka" => new KafkaTransport(connectionString),
        _ => throw new ArgumentOutOfRangeException(
            nameof(kind), kind, "Messaging:Kind must be 'RabbitMq', 'RabbitMqStream', or 'Kafka'."),
    };
}
