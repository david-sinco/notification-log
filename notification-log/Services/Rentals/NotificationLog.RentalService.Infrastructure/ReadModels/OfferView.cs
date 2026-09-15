using NotificationLog.RentalService.Domain.Offers.Enums;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class OfferView
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid OffererId { get; set; }
    public Operation Operation { get; set; }
    public OfferStatus Status { get; set; }
    public long ListedPrice { get; set; }
    public long Amount { get; set; }
    public DateOnly? RentStartDate { get; set; }
    public int? RentTermMonths { get; set; }
    public OfferParty LastMoveBy { get; set; }
    public int CounterOfferCount { get; set; }
    public DateTimeOffset RespondBy { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public List<OfferMoveView> History { get; set; } = [];
}
