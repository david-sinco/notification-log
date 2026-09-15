namespace NotificationLog.RentalService.Application.Favorites.Commands.AddFavorite;

public sealed record AddFavoriteCommand(Guid UserId, Guid ListingId);
