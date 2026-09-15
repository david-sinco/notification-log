using NotificationLog.RentalService.Domain.Offers.Enums;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class OfferDecision
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid OffererId { get; set; }
    public OfferStatus Status { get; set; }
    public bool IsOpen { get; set; }
}
