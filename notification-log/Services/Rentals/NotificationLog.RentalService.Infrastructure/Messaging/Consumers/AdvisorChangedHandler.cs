using Marten;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Infrastructure.IdentityReplica;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Consumers;

public static class AdvisorChangedHandler
{
    public static Task Handle(AdvisorChanged message, IDocumentSession session, CancellationToken ct)
    {
        session.Store(new AdvisorDocument
        {
            Id = Guid.Parse(message.AdvisorId),
            IsActive = message.IsActive,
            ServiceCities = [.. message.ServiceCities],
            Capacity = message.Capacity
        });

        return session.SaveChangesAsync(ct);
    }
}
