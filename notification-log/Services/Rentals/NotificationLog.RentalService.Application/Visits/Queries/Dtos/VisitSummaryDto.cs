namespace NotificationLog.RentalService.Application.Visits.Queries.Dtos;

public sealed record VisitSummaryDto(
    Guid Id,
    Guid ListingId,
    Guid VisitorId,
    Guid HostId,
    string Status,
    DateTimeOffset FirstSlotStart,
    DateTimeOffset? ConfirmedSlotStart,
    DateTimeOffset RespondBy,
    DateTimeOffset UpdatedAt);
