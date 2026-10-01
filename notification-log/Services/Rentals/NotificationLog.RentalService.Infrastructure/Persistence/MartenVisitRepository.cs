using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenVisitRepository : IVisitRepository
{
    private readonly AggregateStreams _streams;

    public MartenVisitRepository(AggregateStreams streams) => _streams = streams;

    public Task<Visit?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Visit>(id, cancellationToken);

    public Task AppendAsync(Visit visit, CancellationToken cancellationToken = default)
    {
        _streams.Append(visit);
        return Task.CompletedTask;
    }
}
