using System.Text.RegularExpressions;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Owners.Enums;

namespace NotificationLog.RentalService.Domain.Owners.ValueObjects;

public sealed record IdentityDocument
{
    private static readonly Regex NumericPattern = new(@"^\d{6,10}$", RegexOptions.Compiled);
    private static readonly Regex PassportPattern = new(@"^[A-Z0-9]{5,20}$", RegexOptions.Compiled);

    public DocumentType Type { get; }
    public string Number { get; }

    private IdentityDocument(DocumentType type, string number)
    {
        Type = type;
        Number = number;
    }

    public static IdentityDocument Create(DocumentType type, string number)
    {
        if (!Enum.IsDefined(type))
            throw new DomainException("El tipo de documento no es válido.");

        if (string.IsNullOrWhiteSpace(number))
            throw new DomainException("El número de documento es obligatorio.");

        var normalized = number.Replace(".", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
        var pattern = type == DocumentType.Passport ? PassportPattern : NumericPattern;

        if (!pattern.IsMatch(normalized))
            throw new DomainException("El número de documento no tiene un formato válido.");

        return new IdentityDocument(type, normalized);
    }

    internal static IdentityDocument FromStorage(DocumentType type, string number) => new(type, number);

    public override string ToString() => $"{Type} {Number}";
}
