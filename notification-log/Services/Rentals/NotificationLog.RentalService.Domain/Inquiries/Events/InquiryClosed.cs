using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Inquiries.Events;

public sealed record InquiryClosed(string Reason) : DomainEvent;
