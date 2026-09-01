namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientPhone;

public sealed record UpdateRecipientPhoneCommand(Guid RecipientId, string? Phone);
