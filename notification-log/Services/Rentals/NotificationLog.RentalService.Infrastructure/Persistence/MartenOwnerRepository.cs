using JasperFx;
using Marten;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal sealed class MartenOwnerRepository : IOwnerRepository
{
    private readonly AggregateStreams _streams;
    private readonly IDocumentStore _store;

    public MartenOwnerRepository(AggregateStreams streams, IDocumentStore store)
        => (_streams, _store) = (streams, store);

    public Task<Owner?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Owner>(id, cancellationToken);

    public Task AppendAsync(Owner owner, CancellationToken cancellationToken = default)
    {
        _streams.Append(owner);
        return Task.CompletedTask;
    }

    public Task<bool> TryReserveNaturalAsync(Guid ownerId, IdentityDocument document, CancellationToken cancellationToken = default)
        => TryInsertAsync(new OwnerDocumentReservation { Id = $"{document.Type}:{document.Number}", OwnerId = ownerId }, cancellationToken);

    public Task<bool> TryReserveCompanyAsync(Guid ownerId, Nit nit, CancellationToken cancellationToken = default)
        => TryInsertAsync(new OwnerNitReservation { Id = nit.Number, OwnerId = ownerId }, cancellationToken);

    private async Task<bool> TryInsertAsync<TReservation>(TReservation reservation, CancellationToken ct) where TReservation : notnull
    {
        await using var session = _store.LightweightSession();

        session.Insert(reservation);

        try
        {
            await session.SaveChangesAsync(ct);
            return true;
        }
        catch (DocumentAlreadyExistsException)
        {
            return false;
        }
    }
}
