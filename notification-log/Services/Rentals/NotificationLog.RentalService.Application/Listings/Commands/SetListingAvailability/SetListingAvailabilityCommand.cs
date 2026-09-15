namespace NotificationLog.RentalService.Application.Listings.Commands.SetListingAvailability;

public sealed record SetListingAvailabilityCommand(Guid ActorId, Guid ListingId, bool IsAvailable);
