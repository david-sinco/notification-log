namespace NotificationLog.RentalService.Application.Processes;

public static class NotificationKeys
{
    public const string ListingApproved = "publicacion.aprobada";
    public const string ListingRejected = "publicacion.rechazada";
    public const string ListingExpiresSoon = "publicacion.vence.pronto";
    public const string ListingExpired = "publicacion.vencida";
    public const string ListingSuspended = "publicacion.suspendida";
    public const string ListingClosed = "publicacion.cerrada";
    public const string VisitRequested = "visita.solicitada";
    public const string VisitConfirmed = "visita.confirmada";
    public const string VisitDeclined = "visita.rechazada";
    public const string VisitCancelled = "visita.cancelada";
    public const string VisitReminder = "visita.recordatorio";
}
