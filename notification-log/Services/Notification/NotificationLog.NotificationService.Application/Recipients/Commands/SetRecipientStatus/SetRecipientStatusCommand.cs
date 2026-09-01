namespace NotificationLog.NotificationService.Application.Recipients.Commands.SetRecipientStatus;

public sealed record SetRecipientStatusCommand(Guid RecipientId, bool IsActive);
