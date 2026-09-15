using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users.Events;

/// <summary>Alta del usuario. Es siempre el primer evento del flujo.</summary>
/// <remarks>
/// Es el único evento que lleva el id del usuario: los demás lo toman del flujo, pero este es
/// el que establece la identidad, y al aplicarlo el agregado deja de estar vacío y pasa a
/// saber quién es.
/// El teléfono viaja como cadena vacía cuando el usuario se registra sin teléfono, en lugar de
/// como null: mismo criterio que el contrato UserContactUpdated del servicio de notificaciones
/// (vacío = "no hay valor", nunca "no cambia"), para que traducir de uno a otro sea directo.
/// </remarks>
public sealed record UserRegistered(
    Guid UserId,
    string Name,
    string Email,
    string Phone
) : DomainEvent;
