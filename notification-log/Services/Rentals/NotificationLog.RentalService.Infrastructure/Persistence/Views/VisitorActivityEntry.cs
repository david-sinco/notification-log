using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Views;

public sealed class VisitorActivityEntry
{
    public DateTimeOffset At { get; set; }
    public string Action { get; set; } = string.Empty;
    public VisitParty? By { get; set; }
    public Guid? VisitId { get; set; }
    public string? ListingNeighborhood { get; set; }
    public string? HostName { get; set; }
    public DateTimeOffset? Slot { get; set; }
    public bool IsLate { get; set; }
}
