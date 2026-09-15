using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class VisitView
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid VisitorId { get; set; }
    public Guid HostId { get; set; }
    public VisitStatus Status { get; set; }
    public List<DateTimeOffset> SlotStarts { get; set; } = [];
    public int SlotDurationMinutes { get; set; }
    public DateTimeOffset FirstSlotStart { get; set; }
    public DateTimeOffset? ConfirmedSlotStart { get; set; }
    public DateTimeOffset RespondBy { get; set; }
    public VisitParty? CancelledBy { get; set; }
    public bool IsLateCancellation { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
