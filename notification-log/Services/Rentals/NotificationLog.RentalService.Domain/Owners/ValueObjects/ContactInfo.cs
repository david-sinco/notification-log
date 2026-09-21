using System.Text.RegularExpressions;
using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Owners.ValueObjects;

public sealed record ContactInfo
{
    public const int MaxEmailLength = 256;
    public const string DefaultCountryCode = "+57";

    private static readonly Regex EmailPattern =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PhonePattern =
        new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public string Email { get; }
    public string Phone { get; }

    private ContactInfo(string email, string phone)
    {
        Email = email;
        Phone = phone;
    }

    public static ContactInfo Create(string email, string phone)
        => new(NormalizeEmail(email), NormalizePhone(phone));

    internal static ContactInfo FromStorage(string email, string phone) => new(email, phone);

    public override string ToString() => $"{Email} · {Phone}";

    private static string NormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("El correo del propietario es obligatorio.");

        var normalized = email.Trim().ToLowerInvariant();

        if (normalized.Length > MaxEmailLength || !EmailPattern.IsMatch(normalized))
            throw new DomainException("El correo no tiene un formato válido.");

        return normalized;
    }

    private static string NormalizePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new DomainException("El teléfono del propietario es obligatorio.");

        if (phone.Any(char.IsLetter))
            throw new DomainException("El teléfono no tiene un formato válido.");

        var normalized = new string([.. phone.Where(c => char.IsAsciiDigit(c) || c == '+')]);

        if (normalized.StartsWith("00", StringComparison.Ordinal))
            normalized = string.Concat("+", normalized.AsSpan(2));
        else if (!normalized.StartsWith('+'))
            normalized = DefaultCountryCode + normalized;

        if (!PhonePattern.IsMatch(normalized))
            throw new DomainException("El teléfono no tiene un formato válido.");

        return normalized;
    }
}
