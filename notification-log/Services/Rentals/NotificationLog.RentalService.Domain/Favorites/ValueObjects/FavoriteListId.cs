using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Favorites.ValueObjects;

public static class FavoriteListId
{
    private static readonly Guid Namespace = new("3a9e5b7c-1d24-4f6a-8c0b-7e2f9d1a6b53");

    public static Guid For(Guid userId) => NameBasedGuid.Create(Namespace, userId.ToString("N"));
}
