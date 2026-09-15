using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingPriceChanged(long OldPrice, long NewPrice) : DomainEvent;
