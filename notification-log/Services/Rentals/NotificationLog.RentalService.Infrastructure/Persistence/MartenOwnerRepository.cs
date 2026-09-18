using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenOwnerRepository : IOwnerRepository
{
    private readonly AggregateStreams _streams;

    public MartenOwnerRepository(AggregateStreams streams) => _streams = streams;

    public Task<Owner?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Owner>(id, cancellationToken);

    public Task AppendAsync(Owner owner, CancellationToken cancellationToken = default)
    {
        _streams.Append(owner);
        return Task.CompletedTask;
    }
}
