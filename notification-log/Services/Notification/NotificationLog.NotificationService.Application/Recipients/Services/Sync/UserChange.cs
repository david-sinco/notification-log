namespace NotificationLog.NotificationService.Application.Recipients.Services.Sync;

public sealed record UserChange(
    Guid UserId,
    string? Name,
    string? Email,
    string? Phone,
    string? Locale,
    string? TimeZone,
    bool IsActive,
    IReadOnlyDictionary<string, string?>? Attributes);
