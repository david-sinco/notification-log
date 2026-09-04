namespace NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Push;

public sealed class PushNotificationOptions
{
    public const string SectionName = "Notifications:Push";

    // A diferencia de Email y SMS (ambos contra Buggregator), todavía no hay un simulador de push en el
    // AppHost — BaseUrl queda apuntando a un placeholder de dev hasta que se elija un proveedor
    // (FCM, OneSignal, un sink propio) y se reemplace acá, en Notifications:Push.
    public string BaseUrl { get; set; } = "http://localhost:5299";
    public string Path { get; set; } = "/push";
    public string? ApiKey { get; set; }
}
