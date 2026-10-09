namespace NotificationLog.Web.Api.Rentals.Visitors;

public sealed record VisitorVisitDto(
    Guid VisitId,
    Guid ListingId,
    string? ListingType,
    string? ListingNeighborhood,
    string? ListingCity,
    string HostName,
    string Status,
    DateTimeOffset? FirstSlot,
    DateTimeOffset? RespondBy,
    DateTimeOffset RequestedAt,
    DateTimeOffset UpdatedAt);
