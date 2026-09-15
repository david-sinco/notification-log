using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;

public static class SavedSearchListId
{
    private static readonly Guid Namespace = new("c7b1e4d2-5f38-4a9c-b6e0-1d8a3f5c7e29");

    public static Guid For(Guid userId) => NameBasedGuid.Create(Namespace, userId.ToString("N"));
}
