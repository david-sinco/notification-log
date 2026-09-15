using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users.Events;

/// <summary>
/// El usuario pidió cambiar su correo. Al aplicarse tumba la confirmación previa: un correo
/// nuevo nunca está verificado, y esa consecuencia se deduce del evento en vez de emitir
/// además un "EmailUnconfirmed" que sería redundante.
/// </summary>
public sealed record EmailChanged(string Email) : DomainEvent;
