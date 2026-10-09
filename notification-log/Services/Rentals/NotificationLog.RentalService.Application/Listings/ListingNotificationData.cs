using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Listings;

public static class ListingNotificationData
{
    private static readonly TimeZoneInfo Colombia = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota");

    public static Dictionary<string, string> For(Listing listing) => new()
    {
        ["listing_id"] = listing.Id.ToString(),
        ["listing_title"] = TitleOf(listing),
        ["listing_address"] = listing.Location?.Address ?? "",
        ["listing_price"] = PriceOf(listing.Price)
    };

    public static string PriceOf(Money? price) => price?.ToString() ?? "Sin precio definido";

    public static string DateOf(DateTimeOffset value) => $"{TimeZoneInfo.ConvertTime(value, Colombia):yyyy-MM-dd}";

    public static string LabelOf(RejectionReason reason) => reason switch
    {
        RejectionReason.LowQualityPhotos => "Fotos de baja calidad",
        RejectionReason.InconsistentData => "Datos inconsistentes",
        RejectionReason.SuspiciousPrice => "Precio sospechoso",
        RejectionReason.InvalidAddress => "Dirección inválida",
        RejectionReason.ProhibitedContent => "Contenido prohibido",
        _ => reason.ToString()
    };

    private static string TitleOf(Listing listing)
    {
        var operation = listing.Operation == Operation.Rent ? "arriendo" : "venta";

        if (listing.Details is null || listing.Location is null)
            return $"Publicación en {operation}";

        var type = listing.Details.Type == PropertyType.Studio ? "Apartaestudio" : "Apartamento";

        return $"{type} en {operation} · {listing.Location.Neighborhood}, {listing.Location.City}";
    }
}
