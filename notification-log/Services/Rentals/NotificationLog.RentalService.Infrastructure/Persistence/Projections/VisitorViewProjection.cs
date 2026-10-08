using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Projections;
using NotificationLog.RentalService.Domain.Visitors.Events;
using NotificationLog.RentalService.Domain.Visits.Events;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Projections;

public sealed class VisitorViewProjection : MultiStreamProjection<VisitorView, Guid>
{
    public VisitorViewProjection()
    {
        Identity<VisitorRegistered>(e => e.VisitorId);
        Identity<VisitorUpdated>(e => e.VisitorId);
        Identity<VisitRequested>(e => e.VisitorId);
    }

    public override VisitorView? Evolve(VisitorView? snapshot, Guid id, IEvent e)
    {
        switch (e.Data)
        {
            case VisitorRegistered r:
                return new VisitorView
                {
                    Id = id,
                    Name = r.Name,
                    Email = r.Email,
                    Phone = r.Phone,
                    RegisteredAt = EventTime.Of((IDomainEvent)e.Data)
                };
            case VisitorUpdated u when snapshot is not null:
                snapshot.Name = u.Name;
                snapshot.Email = u.Email;
                snapshot.Phone = u.Phone;
                return snapshot;
            case VisitRequested when snapshot is not null:
                snapshot.VisitCount++;
                return snapshot;
            default:
                return snapshot;
        }
    }
}
