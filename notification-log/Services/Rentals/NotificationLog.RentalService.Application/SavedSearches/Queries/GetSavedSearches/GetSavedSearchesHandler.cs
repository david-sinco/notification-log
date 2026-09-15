using NotificationLog.RentalService.Application.Abstractions;

namespace NotificationLog.RentalService.Application.SavedSearches.Queries.GetSavedSearches;

public sealed class GetSavedSearchesHandler
{
    private readonly IUserCollectionsReadModel _collections;

    public GetSavedSearchesHandler(IUserCollectionsReadModel collections) => _collections = collections;

    public Task<IReadOnlyList<SavedSearchDto>> HandleAsync(GetSavedSearchesQuery query, CancellationToken ct)
        => _collections.GetSavedSearchesAsync(query.UserId, ct);
}
