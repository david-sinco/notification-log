namespace NotificationLog.RentalService.Application.Abstractions;

public sealed record SavedSearchDigest(Guid UserId, string SearchName, IReadOnlyList<Guid> ListingIds);
