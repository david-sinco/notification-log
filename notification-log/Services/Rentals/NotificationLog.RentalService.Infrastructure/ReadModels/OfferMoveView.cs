using NotificationLog.RentalService.Domain.Offers.Enums;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class OfferMoveView
{
    public string Kind { get; set; } = string.Empty;
    public OfferParty? By { get; set; }
    public long? Amount { get; set; }
    public string? Reason { get; set; }
    public DateTimeOffset At { get; set; }
}
