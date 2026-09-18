namespace NotificationLog.RentalService.Domain.Owners;

public interface IOwnerRepository
{
    Task<Owner?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Owner owner, CancellationToken cancellationToken = default);
}
