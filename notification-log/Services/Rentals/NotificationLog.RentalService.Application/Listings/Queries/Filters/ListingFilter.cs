using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Listings.Queries.Filters;

public sealed record ListingFilter(
    string? Search = null,
    ListingStatus? Status = null,
    Operation? Operation = null,
    Guid? ParticipantId = null);
