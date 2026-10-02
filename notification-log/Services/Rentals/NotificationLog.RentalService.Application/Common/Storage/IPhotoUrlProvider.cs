namespace NotificationLog.RentalService.Application.Common.Storage;

public interface IPhotoUrlProvider
{
    string ReadUrlFor(Guid listingId, string fileName);
}
