using NotificationLog.RentalService.Domain.SavedSearches;

namespace NotificationLog.RentalService.Api.Contracts.SavedSearches;

public sealed record CreateSavedSearchRequest(string Name, SearchCriteriaRequest Criteria, AlertFrequency Frequency);
