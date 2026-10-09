namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed record VisitorsSummaryDto(int Total, int NewLastWeek, int WithoutVisits);
