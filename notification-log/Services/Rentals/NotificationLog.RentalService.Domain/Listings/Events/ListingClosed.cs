using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingClosed(long FinalPrice, DateOnly SignedOn) : DomainEvent;
