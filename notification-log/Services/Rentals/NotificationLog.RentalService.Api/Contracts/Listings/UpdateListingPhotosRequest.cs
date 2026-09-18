namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record UpdateListingPhotosRequest(IReadOnlyList<string> Photos);
