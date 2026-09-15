using Marten;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Infrastructure.IdentityReplica;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Consumers;

public static class AlertsConsentChangedHandler
{
    public static Task Handle(AlertsConsentChanged message, IDocumentSession session, CancellationToken ct)
    {
        session.Store(new AlertsConsentDocument
        {
            Id = Guid.Parse(message.UserId),
            IsGranted = message.IsGranted
        });

        return session.SaveChangesAsync(ct);
    }
}
