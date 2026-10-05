namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;

public sealed record UpdateRecipientCommand(
    Guid RecipientId,
    string? Name,
    string? Email,
    bool IsEmailVerified,
    string? Phone,
    bool IsPhoneVerified,
    string? Locale,
    string? TimeZone,
    bool? AcceptsNotifications);
