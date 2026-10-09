namespace NotificationLog.RentalService.Domain.Listings;

public static class ListingNotificationKeys
{
    public const string Drafted = "publicacion.creada";
    public const string DetailsUpdated = "publicacion.datos.actualizados";
    public const string PhotosUpdated = "publicacion.fotos.actualizadas";
    public const string PriceChanged = "publicacion.precio.cambiado";
    public const string SubmittedForReview = "publicacion.en.revision";
    public const string Approved = "publicacion.aprobada";
    public const string Rejected = "publicacion.rechazada";
    public const string Paused = "publicacion.pausada";
    public const string Resumed = "publicacion.reanudada";
    public const string Renewed = "publicacion.renovada";
    public const string Expired = "publicacion.vencida";
    public const string Closed = "publicacion.cerrada";
    public const string Withdrawn = "publicacion.retirada";
    public const string Suspended = "publicacion.suspendida";
    public const string Reinstated = "publicacion.rehabilitada";
}
