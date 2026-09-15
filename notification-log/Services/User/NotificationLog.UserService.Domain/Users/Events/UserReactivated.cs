using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users.Events;

/// <summary>
/// Alta de nuevo tras una baja. No lleva ningún campo: el hecho, el flujo al que pertenece y
/// la fecha que aporta DomainEvent son toda la información que hay que dar.
/// </summary>
public sealed record UserReactivated : DomainEvent;
