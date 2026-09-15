using NotificationLog.RentalService.Domain.SavedSearches;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenSavedSearchListRepository : ISavedSearchListRepository
{
    private readonly AggregateStreams _streams;

    public MartenSavedSearchListRepository(AggregateStreams streams) => _streams = streams;

    public Task<SavedSearchList?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<SavedSearchList>(id, cancellationToken);

    public Task AppendAsync(SavedSearchList savedSearchList, CancellationToken cancellationToken = default)
    {
        _streams.Append(savedSearchList);
        return Task.CompletedTask;
    }
}
