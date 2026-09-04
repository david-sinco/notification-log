namespace NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Sms;

public sealed class SmsNotificationOptions
{
    public const string SectionName = "Notifications:Sms";

    // Respaldo para cuando no se corre bajo el AppHost de Aspire (que inyecta la dirección real y
    // actual del contenedor — ver NotificationSendingExtensions.AddNotificationSending). Corriendo
    // vía Aspire este valor no se usa: el puerto real lo asigna Docker en cada corrida y no
    // necesariamente es 8000.
    public string BaseUrl { get; set; } = "http://localhost:8000";

    // /sms/{project} autodetecta el proveedor a partir del payload (from/to/message) y agrupa los
    // mensajes bajo ese proyecto en la UI de Buggregator — no hace falta imitar el shape exacto de
    // un proveedor puntual (Twilio, Vonage, etc.) solo para validar que el envío ocurrió.
    public string Path { get; set; } = "/sms/notification-log";
    public string From { get; set; } = "NotificationLog";
}
