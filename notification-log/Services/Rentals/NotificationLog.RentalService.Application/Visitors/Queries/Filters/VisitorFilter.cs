namespace NotificationLog.RentalService.Application.Visitors.Queries.Filters;

public sealed record VisitorFilter(string? Search = null, bool? HasVisits = null, VisitorSort Sort = VisitorSort.Recent);
