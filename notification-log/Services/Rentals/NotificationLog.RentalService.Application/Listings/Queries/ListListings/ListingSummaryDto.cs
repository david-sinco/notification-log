namespace NotificationLog.RentalService.Application.Listings.Queries.ListListings;

public sealed record ListingSummaryDto(
    Guid Id,
    string Operation,
    string Status,
    string? Type,
    string? City,
    string? Neighborhood,
    long? Price,
    int? Bedrooms,
    decimal? Area,
    Guid OwnerId,
    Guid CreatedBy,
    DateTimeOffset UpdatedAt);
