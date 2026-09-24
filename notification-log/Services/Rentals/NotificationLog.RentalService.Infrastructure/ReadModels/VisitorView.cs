using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Visitors.Enums;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class VisitorView
{
    public Guid Id { get; set; }
    public VisitorStatus Status { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? FirstNames { get; set; }
    public string? LastNames { get; set; }
    public DocumentType? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; set; }
    public DateTimeOffset? ProfileCompletedAt { get; set; }
}
