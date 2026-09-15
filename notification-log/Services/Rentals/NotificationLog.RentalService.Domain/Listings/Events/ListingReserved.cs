using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingReserved(Guid OfferId, DateTimeOffset ReservedUntil) : DomainEvent;
