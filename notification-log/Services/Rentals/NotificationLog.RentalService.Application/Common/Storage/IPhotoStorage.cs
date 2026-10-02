namespace NotificationLog.RentalService.Application.Common.Storage;

public interface IPhotoStorage
{
    Task SaveAsync(Guid listingId, string fileName, Stream content, string contentType, CancellationToken ct);

    Task DeleteAsync(Guid listingId, string fileName, CancellationToken ct);
}
