namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingPhotos;

public sealed record UpdateListingPhotosCommand(Guid ActorId, Guid ListingId, IReadOnlyList<string> Photos);
