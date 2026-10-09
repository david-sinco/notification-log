namespace NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

public sealed record VisitorDetailDto(
    VisitorDto Visitor,
    IReadOnlyList<VisitorVisitDto> Visits,
    IReadOnlyList<VisitorActivityDto> Activity);
