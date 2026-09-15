using Marten;
using NotificationLog.RentalService.Application.Abstractions;

namespace NotificationLog.RentalService.Infrastructure.IdentityReplica;

internal sealed class MartenIdentityReplica : IIdentityReplica
{
    private readonly IDocumentSession _session;

    public MartenIdentityReplica(IDocumentSession session) => _session = session;

    public async Task<PersonVerification?> GetPersonAsync(Guid personId, CancellationToken ct)
        => ToVerification(await _session.LoadAsync<PersonVerificationDocument>(personId, ct));

    public async Task<PersonVerification?> GetPersonByUserAsync(Guid userId, CancellationToken ct)
        => ToVerification(await _session.Query<PersonVerificationDocument>()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct));

    public async Task<AdvisorInfo?> GetAdvisorAsync(Guid advisorId, CancellationToken ct)
        => await _session.LoadAsync<AdvisorDocument>(advisorId, ct) is { } advisor
            ? new AdvisorInfo(advisor.Id, advisor.IsActive, advisor.ServiceCities, advisor.Capacity)
            : null;

    public async Task<bool> HasAlertsConsentAsync(Guid userId, CancellationToken ct)
        => await _session.LoadAsync<AlertsConsentDocument>(userId, ct) is { IsGranted: true };

    private static PersonVerification? ToVerification(PersonVerificationDocument? person)
        => person is null
            ? null
            : new PersonVerification(person.Id, person.UserId, person.IsPhoneVerified, person.IsDocumentVerified);
}
