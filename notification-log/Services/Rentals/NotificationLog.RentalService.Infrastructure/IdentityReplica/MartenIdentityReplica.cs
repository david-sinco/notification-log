using Marten;
using NotificationLog.RentalService.Application.Abstractions;

namespace NotificationLog.RentalService.Infrastructure.IdentityReplica;

internal sealed class MartenIdentityReplica : IIdentityReplica
{
    private readonly IDocumentSession _session;

    public MartenIdentityReplica(IDocumentSession session) => _session = session;

    public async Task<PersonVerification?> GetPersonByUserAsync(Guid userId, CancellationToken ct)
        => ToVerification(await _session.Query<PersonVerificationDocument>()
            .FirstOrDefaultAsync(x => x.UserId == userId, ct));

    private static PersonVerification? ToVerification(PersonVerificationDocument? person)
        => person is null
            ? null
            : new PersonVerification(person.Id, person.UserId, person.IsPhoneVerified, person.IsDocumentVerified);
}
