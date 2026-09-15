using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.Inquiries.Events;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class InquiryDecisionProjection : SingleStreamProjection<InquiryDecision, Guid>
{
    public override InquiryDecision? Evolve(InquiryDecision? snapshot, Guid id, IEvent e)
    {
        if (e.Data is InquiryOpened opened)
            return new InquiryDecision
            {
                Id = id,
                ListingId = opened.ListingId,
                SeekerId = opened.SeekerId,
                OpenedAt = EventTime.Of(opened)
            };

        if (snapshot is not null && e.Data is InquiryClosed)
            snapshot.IsClosed = true;

        return snapshot;
    }
}
