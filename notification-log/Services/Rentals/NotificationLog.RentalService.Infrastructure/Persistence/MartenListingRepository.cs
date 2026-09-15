using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenListingRepository : IListingRepository
{
    private readonly AggregateStreams _streams;

    public MartenListingRepository(AggregateStreams streams) => _streams = streams;

    public Task<Listing?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Listing>(id, cancellationToken);

    public Task AppendAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        _streams.Append(listing);
        return Task.CompletedTask;
    }
}
