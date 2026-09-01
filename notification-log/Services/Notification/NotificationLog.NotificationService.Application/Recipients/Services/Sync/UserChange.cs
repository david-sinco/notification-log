namespace NotificationLog.NotificationService.Application.Recipients.Services.Sync;

public sealed record UserChange(
    Guid UserId,
    UserChangeKind Kind,
    string? Name,
    string? Email,
    string? Phone,
    string? Locale,
    string? TimeZone,
    IReadOnlyDictionary<string, string?>? Attributes,
    bool? IsActive,
    DateTime OccurredAt);
