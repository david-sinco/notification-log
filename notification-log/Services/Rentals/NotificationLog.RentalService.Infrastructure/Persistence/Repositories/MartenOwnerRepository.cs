using NotificationLog.RentalService.Infrastructure.Persistence.Reservations;
using Marten;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Repositories;

internal sealed class MartenOwnerRepository : IOwnerRepository
{
    private readonly AggregateStreams _streams;
    private readonly IDocumentSession _session;

    public MartenOwnerRepository(AggregateStreams streams, IDocumentSession session)
        => (_streams, _session) = (streams, session);

    public Task<Owner?> LoadAsync(Guid id, CancellationToken cancellationToken = default)
        => _streams.LoadAsync<Owner>(id, cancellationToken);

    public Task AppendAsync(Owner owner, CancellationToken cancellationToken = default)
    {
        _streams.Append(owner);
        return Task.CompletedTask;
    }

    public Task<bool> TryReserveEmailAsync(Guid ownerId, string email, CancellationToken cancellationToken = default)
        => TryInsertAsync(email, new OwnerEmailReservation { Id = email, OwnerId = ownerId }, cancellationToken);

    public Task<bool> TryReservePhoneAsync(Guid ownerId, string phone, CancellationToken cancellationToken = default)
        => TryInsertAsync(phone, new OwnerPhoneReservation { Id = phone, OwnerId = ownerId }, cancellationToken);

    public Task<bool> TryReserveUserAsync(Guid ownerId, Guid userId, CancellationToken cancellationToken = default)
    {
        var id = userId.ToString();
        return TryInsertAsync(id, new OwnerUserReservation { Id = id, OwnerId = ownerId }, cancellationToken);
    }

    private async Task<bool> TryInsertAsync<TReservation>(string id, TReservation reservation, CancellationToken ct) where TReservation : notnull
    {
        if (await _session.LoadAsync<TReservation>(id, ct) is not null)
            return false;

        _session.Insert(reservation);
        return true;
    }
}
