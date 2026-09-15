namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class InquiryDecision
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid SeekerId { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public bool IsClosed { get; set; }
}
