using Domain.Shared.EventSourcing;
using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Inquiries.Events;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class InquiryViewProjection : SingleStreamProjection<InquiryView, Guid>
{
    public override InquiryView? Evolve(InquiryView? snapshot, Guid id, IEvent e)
    {
        var at = EventTime.Of((IDomainEvent)e.Data);

        if (e.Data is InquiryOpened o)
            return new InquiryView
            {
                Id = id,
                ListingId = o.ListingId,
                SeekerId = o.SeekerId,
                OpenedAt = at,
                LastMessageAt = at,
                Messages = [new InquiryMessageView { AuthorId = o.SeekerId, Text = o.Message, At = at }]
            };

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case InquiryMessagePosted m:
                snapshot.Messages.Add(new InquiryMessageView { AuthorId = m.AuthorId, Text = m.Message, At = at });
                snapshot.LastMessageAt = at;
                break;
            case InquiryClosed c:
                snapshot.IsClosed = true;
                snapshot.ClosedReason = c.Reason;
                break;
        }

        return snapshot;
    }
}
