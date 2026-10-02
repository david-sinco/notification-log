namespace NotificationLog.RentalService.Application.Listings.Commands.ReorderListingPhotos;

public sealed record ReorderListingPhotosCommand(Guid ListingId, IReadOnlyList<string> FileNames);
