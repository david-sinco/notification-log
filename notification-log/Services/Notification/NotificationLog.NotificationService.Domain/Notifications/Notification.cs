using Domain.Shared.Common;
using Domain.Shared.Exceptions;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Notifications;

public sealed class Notification : AggregateRoot
{
    public const int EventKeyMaxLength = 200;
    public const int DestinationMaxLength = 320;
    public const int ErrorMaxLength = 2000;
    public const int ProviderMessageIdMaxLength = 200;

    private readonly Dictionary<string, string> _payload = [];

    public Guid EventId { get; private set; }
    public string EventKey { get; private set; } = default!;
    public Guid ConfigurationId { get; private set; }
    public Guid TemplateId { get; private set; }
    public Guid TemplateVersionId { get; private set; }
    public Guid RecipientId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public string Destination { get; private set; } = default!;
    public DeliveryStatus Status { get; private set; }
    public string? ProviderMessageId { get; private set; }
    public string? Error { get; private set; }
    public DateTime OccurredAt { get; private set; }

    public IReadOnlyDictionary<string, string> Payload => _payload;

    private Notification() { }   // EF Core

    public static Notification RecordSuccess(
        Guid eventId,
        string eventKey,
        Guid configurationId,
        Guid templateId,
        Guid templateVersionId,
        Guid recipientId,
        NotificationChannel channel,
        string destination,
        string? providerMessageId,
        DateTime occurredAt,
        IReadOnlyDictionary<string, string>? payload = null)
    {
        var notification = Create(
            eventId, eventKey, configurationId, templateId, templateVersionId,
            recipientId, channel, destination, payload);

        notification.Status = DeliveryStatus.Sent;
        notification.ProviderMessageId = providerMessageId is null
            ? null
            : Truncate(providerMessageId.Trim(), ProviderMessageIdMaxLength);
        notification.OccurredAt = occurredAt;

        return notification;
    }

    public static Notification RecordFailure(
        Guid eventId,
        string eventKey,
        Guid configurationId,
        Guid templateId,
        Guid templateVersionId,
        Guid recipientId,
        NotificationChannel channel,
        string destination,
        string error,
        DateTime occurredAt,
        IReadOnlyDictionary<string, string>? payload = null)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new DomainException("El motivo del fallo es obligatorio.");

        var notification = Create(
            eventId, eventKey, configurationId, templateId, templateVersionId,
            recipientId, channel, destination, payload);

        notification.Status = DeliveryStatus.Failed;
        notification.Error = Truncate(error.Trim(), ErrorMaxLength);
        notification.OccurredAt = occurredAt;

        return notification;
    }

    private static Notification Create(
        Guid eventId,
        string eventKey,
        Guid configurationId,
        Guid templateId,
        Guid templateVersionId,
        Guid recipientId,
        NotificationChannel channel,
        string destination,
        IReadOnlyDictionary<string, string>? payload)
    {
        // La referencia al evento de negocio que disparó este envío — permite correlacionar (o
        // deduplicar) una notificación con el mensaje puntual que la originó, algo que EventKey por
        // sí solo no puede: un mismo EventKey se repite en cada evento de ese tipo.
        if (eventId == Guid.Empty)
            throw new DomainException("El identificador del evento de origen es obligatorio.");

        if (string.IsNullOrWhiteSpace(eventKey))
            throw new DomainException("El event key es obligatorio.");

        if (configurationId == Guid.Empty)
            throw new DomainException("La configuración de origen es obligatoria.");

        if (templateVersionId == Guid.Empty)
            throw new DomainException("La versión de plantilla es obligatoria.");

        if (recipientId == Guid.Empty)
            throw new DomainException("El destinatario es obligatorio.");

        if (!Enum.IsDefined(channel))
            throw new DomainException($"El canal '{channel}' no es válido.");

        if (string.IsNullOrWhiteSpace(destination))
            throw new DomainException("No se puede registrar una notificación sin destino.");

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            EventKey = Truncate(eventKey.Trim(), EventKeyMaxLength),
            ConfigurationId = configurationId,
            TemplateId = templateId,
            TemplateVersionId = templateVersionId,
            RecipientId = recipientId,
            Channel = channel,
            Destination = Truncate(destination.Trim(), DestinationMaxLength)
        };

        if (payload is not null)
            foreach (var (key, value) in payload)
                notification._payload[key] = value;

        return notification;
    }

    private static string Truncate(string value, int max)
        => value.Length > max ? value[..max] : value;
}
