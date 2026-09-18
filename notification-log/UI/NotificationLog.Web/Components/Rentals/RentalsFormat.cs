using System.Globalization;
using NotificationLog.Web.Api.Rentals;

namespace NotificationLog.Web.Components.Rentals;

public static class RentalsFormat
{
    public static readonly TimeSpan ColombiaOffset = TimeSpan.FromHours(-5);

    private static readonly CultureInfo Colombia = CultureInfo.GetCultureInfo("es-CO");

    private static readonly Dictionary<string, string> Statuses = new()
    {
        ["Draft"] = "Borrador",
        ["InReview"] = "En revisión",
        ["Published"] = "Publicada",
        ["Paused"] = "Pausada",
        ["Expired"] = "Vencida",
        ["Closed"] = "Cerrada",
        ["Withdrawn"] = "Retirada",
        ["Suspended"] = "Suspendida",
        ["Requested"] = "Solicitada",
        ["Confirmed"] = "Confirmada",
        ["Declined"] = "Rechazada",
        ["Cancelled"] = "Cancelada",
        ["Completed"] = "Realizada",
        ["NoShow"] = "Inasistencia"
    };

    private static readonly Dictionary<string, string> Terms = new()
    {
        ["Sale"] = "Venta",
        ["Rent"] = "Arriendo",
        ["Apartment"] = "Apartamento",
        ["Studio"] = "Apartaestudio",
        ["LowQualityPhotos"] = "Fotos de baja calidad",
        ["InconsistentData"] = "Datos inconsistentes",
        ["SuspiciousPrice"] = "Precio sospechoso",
        ["InvalidAddress"] = "Dirección inválida",
        ["ProhibitedContent"] = "Contenido prohibido",
        ["Visitor"] = "Visitante",
        ["Host"] = "Anfitrión"
    };

    public static string Status(string status) => Statuses.GetValueOrDefault(status, status);

    public static string Term(string? value) => value is null ? "—" : Terms.GetValueOrDefault(value, value);

    public static string Term(Enum value) => Term(value.ToString());

    public static string StatusVariant(string status) => status switch
    {
        "Published" or "Confirmed" or "Completed" => "success",
        "InReview" or "Requested" => "warning",
        "Closed" => "info",
        "Suspended" or "Withdrawn" or "Declined" or "NoShow" => "danger",
        _ => "neutral"
    };

    public static string Money(long? amount) => amount is { } value ? "$" + value.ToString("N0", Colombia) : "—";

    public static string Short(Guid id) => id.ToString("N")[..8];

    public static string Date(DateTimeOffset? value) =>
        value is { } date ? date.ToOffset(ColombiaOffset).ToString("dd/MM/yyyy HH:mm", Colombia) : "—";

    public static string Date(DateOnly? value) => value?.ToString("dd/MM/yyyy", Colombia) ?? "—";

    public static DateTimeOffset FromColombia(DateTime local) =>
        new(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), ColombiaOffset);

    public static string ListingTitle(string? type, string? neighborhood, string? city) =>
        neighborhood is null ? "Publicación sin datos del inmueble" : $"{Term(type)} en {neighborhood}, {city}";

    public static IEnumerable<T> Values<T>() where T : struct, Enum => Enum.GetValues<T>();

    public static Operation ParseOperation(string value) => Enum.Parse<Operation>(value);
}
