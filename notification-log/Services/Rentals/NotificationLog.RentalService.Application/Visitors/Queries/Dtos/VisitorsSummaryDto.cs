namespace NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

public sealed record VisitorsSummaryDto(int Total, int NewLastWeek, int WithoutVisits);
