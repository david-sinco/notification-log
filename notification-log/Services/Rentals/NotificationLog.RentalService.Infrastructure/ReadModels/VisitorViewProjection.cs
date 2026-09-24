using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Visitors.Events;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class VisitorViewProjection : SingleStreamProjection<VisitorView, Guid>
{
    public override VisitorView? Evolve(VisitorView? snapshot, Guid id, IEvent e)
        => e.Data switch
        {
            VisitorRegistered r => new VisitorView
            {
                Id = id,
                FirstNames = r.FirstNames,
                LastNames = r.LastNames,
                DocumentType = r.DocumentType,
                DocumentNumber = r.DocumentNumber,
                Email = r.Email,
                Phone = r.Phone,
                RegisteredAt = EventTime.Of((IDomainEvent)e.Data)
            },
            _ => snapshot
        };
}
