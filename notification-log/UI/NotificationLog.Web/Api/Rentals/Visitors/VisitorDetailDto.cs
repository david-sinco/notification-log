namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed record VisitorDetailDto(
    VisitorDto Visitor,
    IReadOnlyList<VisitorVisitDto> Visits,
    IReadOnlyList<VisitorActivityDto> Activity);
