using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Visitors.Events;

public sealed record VisitorRegistered(
    Guid VisitorId,
    string Name,
    string Email,
    string Phone
) : DomainEvent;
