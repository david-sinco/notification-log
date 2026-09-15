using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class VisitDecision
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid VisitorId { get; set; }
    public Guid HostId { get; set; }
    public VisitStatus Status { get; set; }
    public TimeSpan SlotDuration { get; set; }
    public DateTimeOffset? SlotStart { get; set; }
    public DateTimeOffset? SlotEnd { get; set; }
    public DateTimeOffset? StrikeAt { get; set; }
}
