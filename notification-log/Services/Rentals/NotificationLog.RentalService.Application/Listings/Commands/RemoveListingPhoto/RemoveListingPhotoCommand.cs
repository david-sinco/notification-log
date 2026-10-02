namespace NotificationLog.RentalService.Application.Listings.Commands.RemoveListingPhoto;

public sealed record RemoveListingPhotoCommand(Guid ListingId, string FileName);
