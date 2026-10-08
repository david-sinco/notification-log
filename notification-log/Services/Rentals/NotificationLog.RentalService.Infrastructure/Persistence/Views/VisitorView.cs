namespace NotificationLog.RentalService.Infrastructure.Persistence.Views;

public sealed class VisitorView
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; set; }
    public int VisitCount { get; set; }
}
