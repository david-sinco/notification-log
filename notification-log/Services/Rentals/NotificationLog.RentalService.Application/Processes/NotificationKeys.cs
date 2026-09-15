namespace NotificationLog.RentalService.Application.Processes;

public static class NotificationKeys
{
    public const string ListingApproved = "publicacion.aprobada";
    public const string ListingRejected = "publicacion.rechazada";
    public const string ListingExpiresSoon = "publicacion.vence.pronto";
    public const string ListingExpired = "publicacion.vencida";
    public const string ListingSuspended = "publicacion.suspendida";
    public const string ListingClosed = "publicacion.cerrada";
    public const string InquiryNew = "consulta.nueva";
    public const string InquiryReply = "consulta.respuesta";
    public const string VisitRequested = "visita.solicitada";
    public const string VisitConfirmed = "visita.confirmada";
    public const string VisitDeclined = "visita.rechazada";
    public const string VisitCancelled = "visita.cancelada";
    public const string VisitReminder = "visita.recordatorio";
    public const string OfferReceived = "oferta.recibida";
    public const string OfferCountered = "oferta.contraoferta";
    public const string OfferAccepted = "oferta.aceptada";
    public const string OfferRejected = "oferta.rechazada";
    public const string OfferExpired = "oferta.vencida";
    public const string ReservationReleased = "reserva.liberada";
    public const string FavoritePriceDrop = "favorito.precio.baja";
    public const string FavoriteUnavailable = "favorito.no.disponible";
    public const string SearchMatch = "busqueda.coincidencia";
    public const string SearchDigest = "busqueda.resumen";
}
