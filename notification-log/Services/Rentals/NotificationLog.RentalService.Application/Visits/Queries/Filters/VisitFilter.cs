using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Application.Visits.Queries.Filters;

public sealed record VisitFilter(
    Guid? ListingId = null,
    Guid? ParticipantId = null,
    VisitStatus? Status = null);
