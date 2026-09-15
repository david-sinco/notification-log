using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class ListingView
{
    public Guid Id { get; set; }
    public Guid PublisherId { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid? AdvisorId { get; set; }
    public Operation Operation { get; set; }
    public ListingStatus Status { get; set; }
    public PropertyType? Type { get; set; }
    public decimal? Area { get; set; }
    public int? Bedrooms { get; set; }
    public int? Bathrooms { get; set; }
    public int? ParkingSpots { get; set; }
    public int? Stratum { get; set; }
    public int? Floor { get; set; }
    public bool HasElevator { get; set; }
    public long? AdministrationFee { get; set; }
    public string? City { get; set; }
    public string? Neighborhood { get; set; }
    public string? Address { get; set; }
    public string? Description { get; set; }
    public long? Price { get; set; }
    public List<string> Photos { get; set; } = [];
    public DateTimeOffset? ExpiresAt { get; set; }
    public Guid? ReservedOfferId { get; set; }
    public DateTimeOffset? ReservedUntil { get; set; }
    public bool IsReservationExtended { get; set; }
    public List<Guid> ReporterIds { get; set; } = [];
    public List<RejectionReason> RejectionReasons { get; set; } = [];
    public string? StatusReason { get; set; }
    public long? FinalPrice { get; set; }
    public DateOnly? SignedOn { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
