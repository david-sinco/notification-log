namespace NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;

public sealed record ChangeListingPriceCommand(Guid ActorId, Guid ListingId, long Price);
