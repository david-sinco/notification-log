using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Visits.Events;

public sealed record VisitRequested(
    Guid VisitId,
    Guid ListingId,
    Guid HostId,
    Guid VisitorId,
    IReadOnlyList<DateTimeOffset> Slots,
    string? Note,
    DateTimeOffset RespondBy) : DomainEvent;
