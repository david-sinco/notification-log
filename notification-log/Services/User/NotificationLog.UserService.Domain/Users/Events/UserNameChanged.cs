using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users.Events;

public sealed record UserNameChanged(string Name) : DomainEvent;
