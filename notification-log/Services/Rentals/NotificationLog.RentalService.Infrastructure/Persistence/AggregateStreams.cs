using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Marten;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class AggregateStreams
{
    private readonly IDocumentSession _session;
    private readonly Dictionary<Guid, long> _versions = [];

    public AggregateStreams(IDocumentSession session) => _session = session;

    public async Task<TAggregate?> LoadAsync<TAggregate>(Guid id, CancellationToken ct) where TAggregate : AggregateRoot
    {
        var events = await _session.Events.FetchStreamAsync(id, token: ct);

        if (events.Count == 0)
            return null;

        _versions[id] = events[^1].Version;

        return AggregateRoot.Rehydrate<TAggregate>(id, events.Select(e => (IDomainEvent)e.Data).ToList());
    }

    public void Append<TAggregate>(TAggregate aggregate) where TAggregate : AggregateRoot
    {
        var events = aggregate.DomainEvents.Cast<object>().ToArray();

        if (events.Length == 0)
            return;

        if (_versions.TryGetValue(aggregate.Id, out var version))
            _session.Events.Append(aggregate.Id, version + events.Length, events);
        else
            _session.Events.StartStream<TAggregate>(aggregate.Id, events);

        _versions[aggregate.Id] = version + events.Length;
        aggregate.ClearDomainEvents();
    }
}
