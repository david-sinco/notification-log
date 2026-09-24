using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Visitors.Enums;
using NotificationLog.RentalService.Domain.Visitors.Events;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class VisitorViewProjection : SingleStreamProjection<VisitorView, Guid>
{
    public override VisitorView? Evolve(VisitorView? snapshot, Guid id, IEvent e)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        switch (e.Data)
        {
            case VisitorRegistered r:
                return new VisitorView
                {
                    Id = id,
                    Status = VisitorStatus.PendingProfile,
                    DisplayName = r.Name,
                    Email = r.Email,
                    Phone = r.Phone,
                    RegisteredAt = at
                };

            case VisitorProfileCompleted c when snapshot is not null:
                snapshot.Status = VisitorStatus.Registered;
                snapshot.DisplayName = $"{c.FirstNames} {c.LastNames}";
                snapshot.FirstNames = c.FirstNames;
                snapshot.LastNames = c.LastNames;
                snapshot.DocumentType = c.DocumentType;
                snapshot.DocumentNumber = c.DocumentNumber;
                snapshot.Email = c.Email;
                snapshot.Phone = c.Phone;
                snapshot.ProfileCompletedAt = at;
                return snapshot;

            default:
                return snapshot;
        }
    }
}
