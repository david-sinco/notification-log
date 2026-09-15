namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record CloseListingRequest(Guid ActorId, long FinalPrice, DateOnly SignedOn);
