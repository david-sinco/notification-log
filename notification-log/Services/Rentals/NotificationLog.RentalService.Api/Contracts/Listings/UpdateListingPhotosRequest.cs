namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record UpdateListingPhotosRequest(Guid ActorId, IReadOnlyList<string> Photos);
