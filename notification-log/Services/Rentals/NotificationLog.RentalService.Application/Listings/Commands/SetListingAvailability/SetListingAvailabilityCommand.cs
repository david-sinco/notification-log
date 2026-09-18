namespace NotificationLog.RentalService.Application.Listings.Commands.SetListingAvailability;

public sealed record SetListingAvailabilityCommand(Guid ListingId, bool IsAvailable);
