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
    Guid PublisherId,
    Guid CreatedBy,
    Guid? AdvisorId,
    int ReportCount,
    DateTimeOffset UpdatedAt);

public sealed record ListingDto(
    Guid Id,
    Guid PublisherId,
    Guid CreatedBy,
    Guid? AdvisorId,
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
    Guid? ReservedOfferId,
    DateTimeOffset? ReservedUntil,
    bool IsReservationExtended,
    int ReportCount,
    IReadOnlyList<string> RejectionReasons,
    string? StatusReason,
    long? FinalPrice,
    DateOnly? SignedOn,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record DraftListingRequest(Guid ActorId, Guid PublisherId, Guid? AdvisorId, Operation Operation);

public sealed record UpdateListingDetailsRequest(
    Guid ActorId,
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

public sealed record UpdateListingPhotosRequest(Guid ActorId, IReadOnlyList<string> Photos);

public sealed record ChangeListingPriceRequest(Guid ActorId, long Price);

public sealed record ActorRequest(Guid ActorId);

public sealed record SetListingAvailabilityRequest(Guid ActorId, bool IsAvailable);

public sealed record AssignAdvisorRequest(Guid ActorId, Guid? AdvisorId);

public sealed record ReasonRequest(Guid ActorId, string Reason);

public sealed record ReportListingRequest(Guid ReporterId, ReportReason Reason);

public sealed record CloseListingRequest(Guid ActorId, long FinalPrice, DateOnly SignedOn);

public sealed record ReviewListingRequest(Guid ModeratorId, bool Approve, IReadOnlyList<RejectionReason> Reasons);

public sealed record ModeratorRequest(Guid ModeratorId);

public sealed record ModeratorReasonRequest(Guid ModeratorId, string Reason);

public static class ListingLimits
{
    public const int MinPhotos = 5;
    public const int MaxPhotos = 30;
    public const int MinDescriptionLength = 100;
    public const int MaxDescriptionLength = 2000;
}
