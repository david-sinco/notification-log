using NotificationLog.Web.Api.Rentals.Listings;
using NotificationLog.Web.Api.Rentals.Visits;

namespace NotificationLog.Web.Api.Rentals.Dashboard;

public sealed record DashboardDto(
    int InReview,
    DateTimeOffset? OldestInReviewSince,
    int VisitsAwaitingHost,
    DateTimeOffset? NextVisitRespondBy,
    int MyDrafts,
    int Published,
    int? Owners,
    int? Closed,
    IReadOnlyList<ListingSummaryDto> RecentListings,
    int Visits,
    IReadOnlyList<VisitDto> RecentVisits);
