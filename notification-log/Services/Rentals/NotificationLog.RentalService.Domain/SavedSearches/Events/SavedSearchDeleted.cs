using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.SavedSearches.Events;

public sealed record SavedSearchDeleted(Guid SearchId) : DomainEvent;
