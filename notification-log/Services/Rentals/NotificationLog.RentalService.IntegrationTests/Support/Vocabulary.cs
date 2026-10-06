using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public static class Vocabulary
{
    public static ListingStatus ListingStatusOf(string text) => text switch
    {
        "Borrador" => ListingStatus.Draft,
        "En revisión" => ListingStatus.InReview,
        "Publicada" => ListingStatus.Published,
        "Pausada" => ListingStatus.Paused,
        "Vencida" => ListingStatus.Expired,
        "Cerrada" => ListingStatus.Closed,
        "Retirada" => ListingStatus.Withdrawn,
        "Suspendida" => ListingStatus.Suspended,
        _ => throw new ArgumentOutOfRangeException(nameof(text), text, "Estado de publicación desconocido.")
    };

    public static VisitStatus VisitStatusOf(string text) => text switch
    {
        "Esperando al anfitrión" => VisitStatus.AwaitingHost,
        "Esperando al visitante" => VisitStatus.AwaitingVisitor,
        "Agendada" => VisitStatus.Scheduled,
        "Realizada" => VisitStatus.Completed,
        "No asistió" => VisitStatus.NoShow,
        "Cancelada" => VisitStatus.Cancelled,
        "Vencida" => VisitStatus.Expired,
        _ => throw new ArgumentOutOfRangeException(nameof(text), text, "Estado de visita desconocido.")
    };

    public static Operation OperationOf(string text) => text switch
    {
        "arriendo" => Operation.Rent,
        "venta" => Operation.Sale,
        _ => throw new ArgumentOutOfRangeException(nameof(text), text, "Operación desconocida.")
    };
}
