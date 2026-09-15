using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users.Events;

/// <summary>
/// El usuario demostró que controla ese buzón. El código de un solo uso vive en Redis con su
/// TTL y nunca se persiste aquí: al store solo llega el hecho consumado, que es lo único que
/// sigue siendo cierto pasado el TTL.
/// </summary>
/// <remarks>
/// Lleva el correo confirmado —aunque el agregado ya lo tenga— porque el evento cruza la cola
/// hacia el servicio de notificaciones: si solo llevara el hecho, el productor tendría que ir a
/// buscar la dirección al agregado y podría leer una más nueva que la que se verificó,
/// publicando como confirmada una dirección que nadie ha confirmado.
/// </remarks>
public sealed record EmailConfirmed(string Email) : DomainEvent;
