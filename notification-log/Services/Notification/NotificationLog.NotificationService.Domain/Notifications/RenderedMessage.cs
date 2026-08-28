namespace NotificationLog.NotificationService.Domain.Notifications;

// Esto NO funciona como esperas: dos RenderedMessage con las mismas
// etiquetas dan false, porque compara las referencias de las listas.
public sealed record RenderedMessage(string Subject, string Body);