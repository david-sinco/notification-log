namespace NotificationLog.RentalService.Application.SavedSearches.Commands.DeleteSavedSearch;

public sealed record DeleteSavedSearchCommand(Guid UserId, Guid SearchId);
