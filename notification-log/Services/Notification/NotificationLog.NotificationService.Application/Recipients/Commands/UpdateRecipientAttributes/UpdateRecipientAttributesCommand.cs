namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipientAttributes;

public sealed record UpdateRecipientAttributesCommand(
    Guid RecipientId, IReadOnlyDictionary<string, string?> Attributes);
