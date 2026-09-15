namespace NotificationLog.RentalService.Application.Abstractions;

public interface IIdentityReplica
{
    Task<PersonVerification?> GetPersonAsync(Guid personId, CancellationToken ct);

    Task<PersonVerification?> GetPersonByUserAsync(Guid userId, CancellationToken ct);

    Task<AdvisorInfo?> GetAdvisorAsync(Guid advisorId, CancellationToken ct);

    Task<bool> HasAlertsConsentAsync(Guid userId, CancellationToken ct);
}
