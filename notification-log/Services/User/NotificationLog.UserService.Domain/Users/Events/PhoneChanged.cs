using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users.Events;

public sealed record PhoneChanged(string Phone) : DomainEvent;
