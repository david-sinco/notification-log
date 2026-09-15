using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users.Events;

/// <summary>
/// Baja lógica. El motivo es libre y opcional porque en la práctica lo escribe un
/// administrador, pero queda en la historia, que es justo lo que se le suele pedir al soporte.
/// </summary>
public sealed record UserDeactivated(string? Reason) : DomainEvent;
