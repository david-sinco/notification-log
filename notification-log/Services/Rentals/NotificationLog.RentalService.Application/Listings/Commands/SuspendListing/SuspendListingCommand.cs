namespace NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;

public sealed record SuspendListingCommand(Guid ListingId, string Reason);
