using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.Domain.Visitors.Events;

public sealed record VisitorProfileCompleted(
    Guid VisitorId,
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber,
    string Email,
    string Phone
) : DomainEvent;
