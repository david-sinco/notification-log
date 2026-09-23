using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Listings.Queries.ListListings;

public sealed record ListListingsQuery(
    string? Search = null,
    ListingStatus? Status = null,
    Operation? Operation = null,
    Guid? ParticipantId = null);
