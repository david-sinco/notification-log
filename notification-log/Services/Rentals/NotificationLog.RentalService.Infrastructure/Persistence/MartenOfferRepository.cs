using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenOfferRepository : IOfferRepository
{
    private readonly AggregateStreams _streams;

    public MartenOfferRepository(AggregateStreams streams) => _streams = streams;

    public Task<Offer?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Offer>(id, cancellationToken);

    public Task AppendAsync(Offer offer, CancellationToken cancellationToken = default)
    {
        _streams.Append(offer);
        return Task.CompletedTask;
    }
}
