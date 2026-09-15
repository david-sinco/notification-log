namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record SetListingAvailabilityRequest(Guid ActorId, bool IsAvailable);
