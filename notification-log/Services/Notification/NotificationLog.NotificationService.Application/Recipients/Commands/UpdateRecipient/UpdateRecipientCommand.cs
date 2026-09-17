namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;

public sealed record UpdateRecipientCommand(
    Guid RecipientId,
    string? Email,
    string? Phone);
