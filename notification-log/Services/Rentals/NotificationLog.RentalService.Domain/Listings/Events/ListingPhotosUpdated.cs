using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingPhotosUpdated(IReadOnlyList<string> Photos) : DomainEvent;
