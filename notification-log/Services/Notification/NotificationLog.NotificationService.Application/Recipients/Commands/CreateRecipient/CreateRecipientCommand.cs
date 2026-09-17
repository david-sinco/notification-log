namespace NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;

public sealed record CreateRecipientCommand(
    Guid RecipientId,
    string Name,
    string? Email,
    string? Phone,
    string? Locale,
    string? TimeZone,
    bool AcceptsNotifications);
