namespace NotificationLog.RentalService.Application.Listings.Commands.AddListingPhoto;

public sealed record AddListingPhotoCommand(Guid ListingId, Stream Content);
