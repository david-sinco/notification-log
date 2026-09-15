namespace NotificationLog.RentalService.Application.Listings.Commands.CloseListing;

public sealed record CloseListingCommand(Guid ActorId, Guid ListingId, long FinalPrice, DateOnly SignedOn);
