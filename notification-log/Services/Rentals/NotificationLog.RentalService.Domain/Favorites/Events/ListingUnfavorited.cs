using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Favorites.Events;

public sealed record ListingUnfavorited(Guid ListingId) : DomainEvent;
