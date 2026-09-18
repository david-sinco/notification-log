namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record CloseListingRequest(long FinalPrice, DateOnly SignedOn);
