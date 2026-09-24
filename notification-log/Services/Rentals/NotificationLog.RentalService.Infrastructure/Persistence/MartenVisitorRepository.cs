using Marten;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Visitors;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenVisitorRepository : IVisitorRepository
{
    private readonly AggregateStreams _streams;
    private readonly IDocumentSession _session;

    public MartenVisitorRepository(AggregateStreams streams, IDocumentSession session)
        => (_streams, _session) = (streams, session);

    public Task<Visitor?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Visitor>(id, cancellationToken);

    public Task AppendAsync(Visitor visitor, CancellationToken cancellationToken = default)
    {
        _streams.Append(visitor);
        return Task.CompletedTask;
    }

    public async Task<bool> TryReserveVisitorAsync(Guid visitorId, IdentityDocument document, CancellationToken cancellationToken = default)
    {
        var id = $"{document.Type}:{document.Number}";

        if (await _session.LoadAsync<VisitorDocumentReservation>(id, cancellationToken) is not null)
            return false;

        _session.Insert(new VisitorDocumentReservation { Id = id, VisitorId = visitorId });
        return true;
    }
}
