namespace NotificationLog.NotificationService.Application.Recipients.Dtos;

public sealed record RecipientDto(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string Locale,
    string TimeZone,
    IReadOnlyDictionary<string, string> Attributes,
    bool IsActive);