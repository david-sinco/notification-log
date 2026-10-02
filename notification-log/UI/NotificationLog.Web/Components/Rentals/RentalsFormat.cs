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
        ["AwaitingHost"] = "Espera al anfitrión",
        ["AwaitingVisitor"] = "Espera al visitante",
        ["Scheduled"] = "Agendada",
        ["Cancelled"] = "Cancelada",
        ["Completed"] = "Realizada",
        ["NoShow"] = "No asistió",
        ["PendingProfile"] = "Perfil pendiente",
        ["Registered"] = "Registrado"
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
        ["Host"] = "Anfitrión",
        ["System"] = "Sistema"
    };

    public static string Status(string status) => Statuses.GetValueOrDefault(status, status);

    public static string Term(string? value) => value is null ? "—" : Terms.GetValueOrDefault(value, value);

    public static string Term(Enum value) => Term(value.ToString());

    public static string StatusVariant(string status) => status switch
    {
        "Published" or "Scheduled" or "Registered" => "success",
        "InReview" or "AwaitingVisitor" => "info",
        "Paused" or "Expired" or "AwaitingHost" or "PendingProfile" => "warning",
        "Closed" or "Completed" => "ink",
        "Suspended" or "Withdrawn" or "NoShow" or "Cancelled" => "danger",
        _ => "neutral"
    };

    public static string Money(long? amount) => amount is { } value ? "$" + value.ToString("N0", Colombia) : "—";

    public static string Short(Guid id) => id.ToString("N")[..8];

    public static string Date(DateTimeOffset? value) =>
        value is { } date ? date.ToOffset(ColombiaOffset).ToString("dd/MM/yyyy HH:mm", Colombia) : "—";

    public static string Day(DateTimeOffset value) =>
        value.ToOffset(ColombiaOffset).ToString("ddd d MMM", Colombia).Replace(".", "");

    public static string Moment(DateTimeOffset? value) =>
        value is { } date ? $"{Day(date)}, {date.ToOffset(ColombiaOffset):HH:mm}" : "—";

    public static string Range(DateTimeOffset start) =>
        $"{start.ToOffset(ColombiaOffset):HH:mm} – {start.ToOffset(ColombiaOffset).AddHours(1):HH:mm}";

    public static DateTime Today => DateTimeOffset.UtcNow.ToOffset(ColombiaOffset).Date;

    public static string Initials(string? name)
    {
        var parts = (name ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return parts.Length == 0 ? "·" : string.Concat(parts.Take(2).Select(part => char.ToUpperInvariant(part[0])));
    }

    public static string Date(DateOnly? value) => value?.ToString("dd/MM/yyyy", Colombia) ?? "—";

    public static DateTimeOffset FromColombia(DateTime local) =>
        new(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), ColombiaOffset);

    public static string ListingTitle(string? type, string? neighborhood, string? city) =>
        neighborhood is null ? "Publicación sin datos del inmueble" : $"{Term(type)} en {neighborhood}, {city}";

    public static IEnumerable<T> Values<T>() where T : struct, Enum => Enum.GetValues<T>();

    public static Operation ParseOperation(string value) => Enum.Parse<Operation>(value);
}
