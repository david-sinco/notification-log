using Domain.Shared.EventSourcing;

namespace NotificationLog.UserService.Domain.Users.Events;

public sealed record PhoneConfirmed(string Phone) : DomainEvent;
