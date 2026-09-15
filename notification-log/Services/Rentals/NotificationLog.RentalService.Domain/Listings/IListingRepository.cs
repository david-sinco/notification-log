namespace NotificationLog.RentalService.Domain.Listings;

public interface IListingRepository
{
    Task<Listing?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Listing listing, CancellationToken cancellationToken = default);
}