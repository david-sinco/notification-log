using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Application.Visits.Queries.ListVisits;

public sealed record ListVisitsQuery(
    Guid? ListingId = null,
    Guid? ParticipantId = null,
    VisitStatus? Status = null);
