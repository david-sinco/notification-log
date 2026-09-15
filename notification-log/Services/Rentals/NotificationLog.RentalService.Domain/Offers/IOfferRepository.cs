namespace NotificationLog.RentalService.Domain.Offers;

public interface IOfferRepository
{
    Task<Offer?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Offer offer, CancellationToken cancellationToken = default);
}
