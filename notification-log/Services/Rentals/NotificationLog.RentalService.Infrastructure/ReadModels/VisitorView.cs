using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class VisitorView
{
    public Guid Id { get; set; }
    public string FirstNames { get; set; } = string.Empty;
    public string LastNames { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; set; }
}
