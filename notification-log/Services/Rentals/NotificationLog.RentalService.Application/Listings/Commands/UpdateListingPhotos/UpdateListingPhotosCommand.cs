namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingPhotos;

public sealed record UpdateListingPhotosCommand(Guid ListingId, IReadOnlyList<string> Photos);
