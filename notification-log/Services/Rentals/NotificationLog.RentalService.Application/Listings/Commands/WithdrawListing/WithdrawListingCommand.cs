namespace NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;

public sealed record WithdrawListingCommand(Guid ActorId, Guid ListingId, string Reason, bool ByModerator);
