using NotificationLog.RentalService.Application.Listings.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Dashboard.Queries.Dtos;

public sealed record DashboardDto(
    int InReview,
    DateTimeOffset? OldestInReviewSince,
    int VisitsAwaitingHost,
    DateTimeOffset? NextVisitRespondBy,
    int MyDrafts,
    int Published,
    int? Owners,
    int? Closed,
    IReadOnlyList<ListingSummaryDto> RecentListings);
