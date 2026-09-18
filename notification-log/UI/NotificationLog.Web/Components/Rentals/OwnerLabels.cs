using NotificationLog.Web.Api.Rentals.Owners;

namespace NotificationLog.Web.Components.Rentals;

public static class OwnerLabels
{
    public static string Name(OwnerDto owner)
        => owner.LegalName ?? $"{owner.FirstNames} {owner.LastNames}";

    public static string Type(string type) => type switch
    {
        "Natural" => "Persona natural",
        "Company" => "Persona jurídica",
        _ => type
    };

    public static string Document(string? type) => type switch
    {
        nameof(DocumentType.CitizenshipCard) => "Cédula de ciudadanía",
        nameof(DocumentType.ForeignerId) => "Cédula de extranjería",
        nameof(DocumentType.Passport) => "Pasaporte",
        _ => type ?? "—"
    };

    public static string Identification(OwnerDto owner)
        => owner.Nit is not null ? $"NIT {owner.Nit}" : $"{Document(owner.DocumentType)} {owner.DocumentNumber}";
}
