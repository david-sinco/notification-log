using Application.Shared.Abstractions;
using Marten;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenUnitOfWork : IUnitOfWork
{
    private readonly IDocumentSession _session;

    public MartenUnitOfWork(IDocumentSession session) => _session = session;

    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        var count = _session.PendingChanges.Streams().Sum(stream => stream.Events.Count);

        await _session.SaveChangesAsync(ct);

        return count;
    }
}
