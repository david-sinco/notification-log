using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class ListingDecision
{
    public Guid Id { get; set; }
    public Guid PublisherId { get; set; }
    public Guid? AdvisorId { get; set; }
    public Operation Operation { get; set; }
    public ListingStatus Status { get; set; }
    public PropertyType? Type { get; set; }
    public string City { get; set; } = string.Empty;
    public string Neighborhood { get; set; } = string.Empty;
    public decimal Area { get; set; }
    public int Bedrooms { get; set; }
    public int Stratum { get; set; }
    public long Price { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}
