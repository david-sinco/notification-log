using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Views;

public sealed class VisitorVisitEntry
{
    public Guid VisitId { get; set; }
    public Guid ListingId { get; set; }
    public PropertyType? ListingType { get; set; }
    public string? ListingNeighborhood { get; set; }
    public string? ListingCity { get; set; }
    public string HostName { get; set; } = string.Empty;
    public VisitStatus Status { get; set; }
    public List<DateTimeOffset> ProposedSlots { get; set; } = [];
    public DateTimeOffset? RespondBy { get; set; }
    public DateTimeOffset? ScheduledStartsAt { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
