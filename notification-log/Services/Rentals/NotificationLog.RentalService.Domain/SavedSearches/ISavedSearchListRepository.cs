namespace NotificationLog.RentalService.Domain.SavedSearches;

public interface ISavedSearchListRepository
{
    Task<SavedSearchList?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(SavedSearchList savedSearchList, CancellationToken cancellationToken = default);
}
