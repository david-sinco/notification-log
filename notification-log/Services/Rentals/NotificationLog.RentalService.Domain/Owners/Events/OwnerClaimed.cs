using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Owners.Events;

public sealed record OwnerClaimed(Guid OwnerId, Guid UserId) : DomainEvent;
