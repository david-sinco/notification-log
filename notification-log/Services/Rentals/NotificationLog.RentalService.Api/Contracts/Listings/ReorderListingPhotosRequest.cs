namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record ReorderListingPhotosRequest(IReadOnlyList<string> FileNames);
