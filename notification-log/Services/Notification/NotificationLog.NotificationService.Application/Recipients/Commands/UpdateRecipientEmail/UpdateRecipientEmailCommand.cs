namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientEmail;

public sealed record UpdateRecipientEmailCommand(Guid RecipientId, string? Email);