using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Visits.Events;

public sealed record VisitRequested(
    Guid VisitId,
    Guid ListingId,
    Guid VisitorId,
    Guid HostId,
    IReadOnlyList<DateTimeOffset> SlotStarts,
    TimeSpan SlotDuration,
    DateTimeOffset RespondBy
) : DomainEvent;
