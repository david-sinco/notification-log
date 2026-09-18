namespace NotificationLog.Web.Api;

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
}

// Espejo de NotificationLog.NotificationService.Domain.Shared.NotificationChannel: mismos valores
// numéricos porque viaja como número en el cuerpo de las peticiones (sin JsonStringEnumConverter).
public enum NotificationChannel
{
    Email = 1,
    Sms = 2,
    Push = 3,
    WhatsApp = 4
}

// Respuesta { "id": guid } que comparten todos los endpoints de creación (trigger, plantilla,
// configuración, versión de plantilla).
internal sealed record CreatedResponse(Guid Id);
