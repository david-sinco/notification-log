namespace NotificationLog.RentalService.Application.Abstractions;

public interface IIdentityReplica
{
    Task<PersonVerification?> GetPersonByUserAsync(Guid userId, CancellationToken ct);
}
