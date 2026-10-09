using System.Text.Json.Serialization;

namespace NotificationLog.RentalService.Application.Listings.Queries.Dtos;

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
    DateTimeOffset UpdatedAt,
    string? OwnerName,
    string? CoverUrl,
    int PhotoCount,
    [property: JsonIgnore] string? CoverFileName = null);
