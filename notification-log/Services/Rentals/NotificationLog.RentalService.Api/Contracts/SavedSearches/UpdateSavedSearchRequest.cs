using NotificationLog.RentalService.Domain.SavedSearches;

namespace NotificationLog.RentalService.Api.Contracts.SavedSearches;

public sealed record UpdateSavedSearchRequest(string Name, SearchCriteriaRequest Criteria, AlertFrequency Frequency);
