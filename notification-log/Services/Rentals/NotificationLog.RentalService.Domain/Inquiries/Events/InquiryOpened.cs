using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Inquiries.Events;

public sealed record InquiryOpened(
    Guid InquiryId,
    Guid ListingId,
    Guid SeekerId,
    string Message
) : DomainEvent;
