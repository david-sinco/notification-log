namespace NotificationLog.RentalService.Application.Abstractions;

public sealed record SavedSearchMatch(Guid UserId, Guid SearchId, string SearchName);
