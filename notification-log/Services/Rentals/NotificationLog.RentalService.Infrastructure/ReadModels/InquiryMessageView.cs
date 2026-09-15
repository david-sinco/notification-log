namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class InquiryMessageView
{
    public Guid AuthorId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTimeOffset At { get; set; }
}
