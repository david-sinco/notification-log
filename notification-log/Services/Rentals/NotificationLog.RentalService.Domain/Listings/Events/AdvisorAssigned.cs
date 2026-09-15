using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record AdvisorAssigned(Guid AdvisorId) : DomainEvent;
