using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.Domain.Owners;

public interface IOwnerRepository
{
    Task<Owner?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Owner owner, CancellationToken cancellationToken = default);

    Task<bool> TryReserveNaturalAsync(Guid ownerId, IdentityDocument document, CancellationToken cancellationToken = default);

    Task<bool> TryReserveCompanyAsync(Guid ownerId, Nit nit, CancellationToken cancellationToken = default);
}
