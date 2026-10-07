namespace NotificationLog.RentalService.Domain.Owners;

public interface IOwnerRepository
{
    Task<Owner?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Owner owner, CancellationToken cancellationToken = default);

    Task<bool> TryReserveEmailAsync(Guid ownerId, string email, CancellationToken cancellationToken = default);

    Task<bool> TryReservePhoneAsync(Guid ownerId, string phone, CancellationToken cancellationToken = default);

    Task<bool> TryReserveUserAsync(Guid ownerId, Guid userId, CancellationToken cancellationToken = default);
}
