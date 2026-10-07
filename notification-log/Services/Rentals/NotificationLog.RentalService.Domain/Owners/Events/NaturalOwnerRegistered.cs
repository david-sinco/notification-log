using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.Domain.Owners.Events;

public sealed record NaturalOwnerRegistered(
    Guid OwnerId,
    Guid CreatedBy,
    string FirstNames,
    string LastNames,
    DocumentType DocumentType,
    string DocumentNumber,
    string Email,
    string Phone,
    Guid? RelatedUserId = null
) : DomainEvent;
