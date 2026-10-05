using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Views;

public sealed class VisitHistoryEntry
{
    public DateTimeOffset At { get; set; }
    public VisitParty By { get; set; }
    public string Action { get; set; } = string.Empty;
    public List<DateTimeOffset> Slots { get; set; } = [];
    public string? Reason { get; set; }
    public bool IsLate { get; set; }
}
