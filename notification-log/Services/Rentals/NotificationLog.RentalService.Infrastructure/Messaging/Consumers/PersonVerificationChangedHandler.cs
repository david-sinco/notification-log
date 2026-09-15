using Marten;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Infrastructure.IdentityReplica;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Consumers;

public static class PersonVerificationChangedHandler
{
    public static Task Handle(PersonVerificationChanged message, IDocumentSession session, CancellationToken ct)
    {
        session.Store(new PersonVerificationDocument
        {
            Id = Guid.Parse(message.PersonId),
            UserId = string.IsNullOrEmpty(message.UserId) ? null : Guid.Parse(message.UserId),
            IsPhoneVerified = message.IsPhoneVerified,
            IsDocumentVerified = message.IsDocumentVerified
        });

        return session.SaveChangesAsync(ct);
    }
}
