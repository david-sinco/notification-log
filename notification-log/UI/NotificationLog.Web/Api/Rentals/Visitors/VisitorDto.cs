namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed record VisitorDto(
    Guid Id,
    string Status,
    string DisplayName,
    string? FirstNames,
    string? LastNames,
    string? DocumentType,
    string? DocumentNumber,
    string Email,
    string Phone,
    DateTimeOffset RegisteredAt,
    DateTimeOffset? ProfileCompletedAt);
