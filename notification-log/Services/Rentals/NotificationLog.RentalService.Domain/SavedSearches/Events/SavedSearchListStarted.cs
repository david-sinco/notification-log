using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.SavedSearches.Events;

public sealed record SavedSearchListStarted(Guid SavedSearchListId, Guid UserId) : DomainEvent;
