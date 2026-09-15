namespace NotificationLog.RentalService.Application.Listings.Commands.RenewListing;

public sealed record RenewListingCommand(Guid ActorId, Guid ListingId);
