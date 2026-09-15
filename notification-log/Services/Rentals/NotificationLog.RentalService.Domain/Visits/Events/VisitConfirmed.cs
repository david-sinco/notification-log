using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Visits.Events;

public sealed record VisitConfirmed(DateTimeOffset SlotStart) : DomainEvent;
