using NotificationLog.RentalService.Domain.SavedSearches;

namespace NotificationLog.RentalService.Application.SavedSearches.Commands.CreateSavedSearch;

public sealed record CreateSavedSearchCommand(Guid UserId, string Name, SearchCriteriaInput Criteria, AlertFrequency Frequency);
