namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class InquiryView
{
    public Guid Id { get; set; }
    public Guid ListingId { get; set; }
    public Guid SeekerId { get; set; }
    public bool IsClosed { get; set; }
    public string? ClosedReason { get; set; }
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset LastMessageAt { get; set; }
    public List<InquiryMessageView> Messages { get; set; } = [];
}
