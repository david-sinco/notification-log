using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Inquiries.Events;

public sealed record InquiryMessagePosted(Guid AuthorId, string Message) : DomainEvent;
