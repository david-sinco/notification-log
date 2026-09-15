namespace NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;

public sealed record ReinstateListingCommand(Guid ModeratorId, Guid ListingId);
