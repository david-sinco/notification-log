using NotificationLog.RentalService.Domain.Owners.Enums;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

public sealed class OwnerView
{
    public Guid Id { get; set; }
    public Guid CreatedBy { get; set; }
    public OwnerType Type { get; set; }
    public string? FirstNames { get; set; }
    public string? LastNames { get; set; }
    public DocumentType? DocumentType { get; set; }
    public string? DocumentNumber { get; set; }
    public string? LegalName { get; set; }
    public string? Nit { get; set; }
    public int? NitCheckDigit { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTimeOffset RegisteredAt { get; set; }
}
