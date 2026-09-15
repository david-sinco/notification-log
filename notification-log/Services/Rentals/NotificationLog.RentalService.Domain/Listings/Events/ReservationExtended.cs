using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ReservationExtended(DateTimeOffset ReservedUntil) : DomainEvent;
