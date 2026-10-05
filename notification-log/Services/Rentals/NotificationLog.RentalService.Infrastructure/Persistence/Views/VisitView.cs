using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Views;

public sealed class VisitView
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid HostId { get; set; }
    public Guid VisitorId { get; set; }
    public PropertyType? ListingType { get; set; }
    public string? ListingNeighborhood { get; set; }
    public string? ListingCity { get; set; }
    public string VisitorName { get; set; } = string.Empty;
    public string HostName { get; set; } = string.Empty;
    public List<VisitHistoryEntry> History { get; set; } = [];
    public VisitStatus Status { get; set; }
    public List<DateTimeOffset> ProposedSlots { get; set; } = [];
    public DateTimeOffset? RespondBy { get; set; }
    public DateTimeOffset? ScheduledStartsAt { get; set; }
    public DateTimeOffset? ScheduledEndsAt { get; set; }
    public VisitParty? CancelledBy { get; set; }
    public string? CancellationReason { get; set; }
    public bool IsLateCancellation { get; set; }
    public VisitParty? ClosedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
