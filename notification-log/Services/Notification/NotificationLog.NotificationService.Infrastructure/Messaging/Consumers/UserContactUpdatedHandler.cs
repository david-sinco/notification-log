using NotificationLog.Contracts.Users;
using NotificationLog.NotificationService.Application.Recipients.Services.Sync;

namespace NotificationLog.NotificationService.Infrastructure.Messaging.Consumers;

public static class UserContactUpdatedHandler
{
    public static Task Handle(UserContactUpdated message, RecipientSyncService syncService, CancellationToken ct)
    {
        var change = MapToUserChange(message); // Protobuf -> el UserChange de Application
        return syncService.HandleAsync(change, ct);
    }

    // email/phone vacíos ya son un valor con significado (sin dato confirmado), no "no vino" —
    // el mensaje es un snapshot completo, así que se pasan tal cual, sin traducir "" a null. Mismo
    // criterio para attributes: una clave ausente del map ya significa "ya no la tiene".
    private static UserChange MapToUserChange(UserContactUpdated message) =>
        new(
            UserId: Guid.Parse(message.UserId),
            Name: message.Name,
            Email: message.Email,
            Phone: message.Phone,
            Locale: message.Locale,
            TimeZone: message.TimeZone,
            Attributes: message.Attributes.ToDictionary(kv => kv.Key, kv => (string?)kv.Value),
            IsActive: message.IsActive);
}
