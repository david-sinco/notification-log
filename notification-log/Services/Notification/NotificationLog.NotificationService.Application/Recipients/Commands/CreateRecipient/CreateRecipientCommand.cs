namespace NotificationLog.NotificationService.Application.Recipients.Commands.CreateRecipient;

// Mismo set de campos que UpdateRecipientCommand — un mensaje de sincronización trae siempre el
// snapshot completo, exista ya el destinatario o no. La única diferencia real entre los dos
// comandos es qué hacen con un campo vacío: Update lo aplica tal cual (vacío = eliminado);
// Create, al no haber un valor previo que preservar, cae en los defaults del agregado
// (Recipient.Create/ChangeLocalization) en vez de perder el dato si sí vino.
public sealed record CreateRecipientCommand(
    Guid RecipientId,
    string Name,
    string? Email,
    string? Phone,
    string? Locale,
    string? TimeZone,
    bool IsActive,
    IReadOnlyDictionary<string, string?>? Attributes);
