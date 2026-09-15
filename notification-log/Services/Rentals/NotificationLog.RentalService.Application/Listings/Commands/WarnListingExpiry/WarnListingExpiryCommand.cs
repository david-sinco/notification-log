namespace NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;

public sealed record WarnListingExpiryCommand(Guid ListingId, DateTimeOffset ExpiresAt);
