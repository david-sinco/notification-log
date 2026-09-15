using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.SavedSearches;
using NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;

namespace NotificationLog.RentalService.Application.SavedSearches.Commands.DeleteSavedSearch;

public sealed class DeleteSavedSearchHandler
{
    private readonly ISavedSearchListRepository _searches;
    private readonly IUnitOfWork _uow;

    public DeleteSavedSearchHandler(ISavedSearchListRepository searches, IUnitOfWork uow)
        => (_searches, _uow) = (searches, uow);

    public async Task HandleAsync(DeleteSavedSearchCommand cmd, CancellationToken ct)
    {
        if (await _searches.LoadAsync(SavedSearchListId.For(cmd.UserId), ct) is not { } searches)
            return;

        searches.Remove(cmd.SearchId);

        await _searches.AppendAsync(searches, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
