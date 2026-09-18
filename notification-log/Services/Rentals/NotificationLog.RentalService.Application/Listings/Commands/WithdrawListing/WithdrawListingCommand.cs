namespace NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;

public sealed record WithdrawListingCommand(Guid ListingId, string Reason);
