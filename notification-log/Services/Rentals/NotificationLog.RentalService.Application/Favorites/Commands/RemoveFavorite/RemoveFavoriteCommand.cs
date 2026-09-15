namespace NotificationLog.RentalService.Application.Favorites.Commands.RemoveFavorite;

public sealed record RemoveFavoriteCommand(Guid UserId, Guid ListingId);
