using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Visitors.Events;

public sealed record VisitorUpdated(
    Guid VisitorId,
    string Name,
    string Email,
    string Phone
) : DomainEvent;
