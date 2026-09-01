namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientProfile;

public sealed record UpdateRecipientProfileCommand(
    Guid RecipientId, string Name, string? Locale, string? TimeZone);
