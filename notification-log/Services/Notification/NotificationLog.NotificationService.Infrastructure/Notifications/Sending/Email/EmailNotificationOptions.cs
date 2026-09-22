namespace NotificationLog.NotificationService.Infrastructure.Notifications.Sending.Email;

public sealed class EmailNotificationOptions
{
    public const string SectionName = "Notifications:Email";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string From { get; set; } = "notificaciones@llave.local";
    public bool EnableSsl { get; set; }
    public string? Username { get; set; }
    public string? Password { get; set; }
}
