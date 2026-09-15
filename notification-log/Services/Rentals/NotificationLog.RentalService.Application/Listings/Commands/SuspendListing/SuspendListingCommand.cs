namespace NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;

public sealed record SuspendListingCommand(Guid ModeratorId, Guid ListingId, string Reason);
