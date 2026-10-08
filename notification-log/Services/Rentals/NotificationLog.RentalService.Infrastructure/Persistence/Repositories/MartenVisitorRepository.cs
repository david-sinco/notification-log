using NotificationLog.RentalService.Domain.Visitors;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Repositories;

internal sealed class MartenVisitorRepository : IVisitorRepository
{
    private readonly AggregateStreams _streams;

    public MartenVisitorRepository(AggregateStreams streams) => _streams = streams;

    public Task<Visitor?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Visitor>(id, cancellationToken);

    public Task AppendAsync(Visitor visitor, CancellationToken cancellationToken = default)
    {
        _streams.Append(visitor);
        return Task.CompletedTask;
    }
}
