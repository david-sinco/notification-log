namespace NotificationLog.NotificationService.Application.Recipients.Dtos;

public sealed record RecipientSummaryDto(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    bool IsActive);