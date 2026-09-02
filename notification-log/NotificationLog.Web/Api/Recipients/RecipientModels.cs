namespace NotificationLog.Web.Api.Recipients;

public sealed record RecipientSummaryDto(Guid Id, string Name, string? Email, string? Phone, bool IsActive);

public sealed record RecipientDto(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string Locale,
    string TimeZone,
    IReadOnlyDictionary<string, string> Attributes,
    bool IsActive);
