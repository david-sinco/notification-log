using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Owners.Events;

public sealed record OwnerRegistered(
    Guid OwnerId,
    Guid CreatedBy,
    string Name,
    string Email,
    string Phone,
    Guid? RelatedUserId = null
) : DomainEvent;
