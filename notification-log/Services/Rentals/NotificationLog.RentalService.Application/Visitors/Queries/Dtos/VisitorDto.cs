namespace NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

public sealed record VisitorDto(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTimeOffset RegisteredAt,
    int VisitCount,
    int NoShowCount,
    VisitorNextStepDto? NextStep);
