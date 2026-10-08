namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed record VisitorDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTimeOffset RegisteredAt,
    int VisitCount);
