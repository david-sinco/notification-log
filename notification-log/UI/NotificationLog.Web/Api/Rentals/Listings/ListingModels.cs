namespace NotificationLog.Web.Api.Rentals.Listings;

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

public sealed record ListingDto(
    Guid Id,
    Guid OwnerId,
    Guid CreatedBy,
    string Operation,
    string Status,
    string? Type,
    decimal? Area,
    int? Bedrooms,
    int? Bathrooms,
    int? ParkingSpots,
    int? Stratum,
    int? Floor,
    bool HasElevator,
    long? AdministrationFee,
    string? City,
    string? Neighborhood,
    string? Address,
    string? Description,
    long? Price,
    IReadOnlyList<string> Photos,
    DateTimeOffset? ExpiresAt,
    IReadOnlyList<string> RejectionReasons,
    string? StatusReason,
    long? FinalPrice,
    DateOnly? SignedOn,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DraftListingRequest(Guid OwnerId, Operation Operation);

public sealed record UpdateListingDetailsRequest(
    PropertyType Type,
    decimal Area,
    int Bedrooms,
    int Bathrooms,
    int ParkingSpots,
    int Stratum,
    int? Floor,
    bool HasElevator,
    long AdministrationFee,
    string City,
    string Neighborhood,
    string Address,
    string Description);

public sealed record UpdateListingPhotosRequest(IReadOnlyList<string> Photos);

public sealed record ChangeListingPriceRequest(long Price);

public sealed record SetListingAvailabilityRequest(bool IsAvailable);

public sealed record ReasonRequest(string Reason);

public sealed record CloseListingRequest(long FinalPrice, DateOnly SignedOn);

public sealed record ReviewListingRequest(bool Approve, IReadOnlyList<RejectionReason> Reasons);

public static class ListingLimits
{
    public const int MinPhotos = 5;
    public const int MaxPhotos = 30;
    public const int MinDescriptionLength = 100;
    public const int MaxDescriptionLength = 2000;
}
