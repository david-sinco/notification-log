namespace NotificationLog.NotificationService.Application.Recipients.Commands.UpdateRecipient;

// Reemplaza a UpdateRecipientEmail/Phone/Profile/Attributes + SetRecipientStatus: un destinatario
// existente se sincroniza como una sola unidad de trabajo (un solo SaveChangesAsync), no como
// varios comandos independientes cada uno con su propio commit.
//
// Es un snapshot completo, no un delta: no existe "este campo no vino en el mensaje". Vacío es un
// valor real — el dato ya no está confirmado — y se aplica tal cual, no se ignora. Única excepción:
// Name nunca puede quedar vacío (el dominio no admite un Recipient sin nombre), así que un Name
// vacío no es un valor a aplicar, es un comando inválido (lo rechaza UpdateRecipientValidator).
public sealed record UpdateRecipientCommand(
    Guid RecipientId,
    string? Name,
    string? Email,
    string? Phone,
    string? Locale,
    string? TimeZone,
    bool IsActive,
    IReadOnlyDictionary<string, string?>? Attributes);
