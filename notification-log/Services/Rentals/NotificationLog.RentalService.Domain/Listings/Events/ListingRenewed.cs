using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingRenewed(DateTimeOffset ExpiresAt) : DomainEvent;
