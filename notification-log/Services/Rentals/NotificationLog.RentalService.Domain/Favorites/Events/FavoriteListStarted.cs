using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Favorites.Events;

public sealed record FavoriteListStarted(Guid FavoriteListId, Guid UserId) : DomainEvent;
