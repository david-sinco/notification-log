using Marten;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Infrastructure.IdentityReplica;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Consumers;

public static class PersonVerificationChangedHandler
{
    public static async Task Handle(PersonVerificationChanged message, IDocumentSession session, CancellationToken ct)
    {
        var userId = Guid.Parse(message.UserId);

        var person = await session.LoadAsync<PersonVerificationDocument>(userId, ct)
            ?? new PersonVerificationDocument { Id = userId, UserId = userId };

        person.IsPhoneVerified = !string.IsNullOrEmpty(message.Phone);

        session.Store(person);
        await session.SaveChangesAsync(ct);
    }
}
