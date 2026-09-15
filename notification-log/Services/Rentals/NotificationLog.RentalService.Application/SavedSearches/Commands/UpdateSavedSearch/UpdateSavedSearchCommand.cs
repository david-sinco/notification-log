using NotificationLog.RentalService.Domain.SavedSearches;

namespace NotificationLog.RentalService.Application.SavedSearches.Commands.UpdateSavedSearch;

public sealed record UpdateSavedSearchCommand(
    Guid UserId,
    Guid SearchId,
    string Name,
    SearchCriteriaInput Criteria,
    AlertFrequency Frequency);
