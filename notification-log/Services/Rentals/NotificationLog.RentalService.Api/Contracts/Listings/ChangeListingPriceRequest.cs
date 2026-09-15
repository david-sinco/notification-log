namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record ChangeListingPriceRequest(Guid ActorId, long Price);
