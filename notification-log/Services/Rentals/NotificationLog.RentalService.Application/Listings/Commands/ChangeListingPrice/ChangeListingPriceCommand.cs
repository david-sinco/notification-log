namespace NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;

public sealed record ChangeListingPriceCommand(Guid ListingId, long Price);
