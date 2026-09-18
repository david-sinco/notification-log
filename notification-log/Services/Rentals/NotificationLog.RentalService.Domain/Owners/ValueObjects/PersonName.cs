using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Owners.ValueObjects;

public sealed record PersonName
{
    public const int MaxLength = 100;

    public string FirstNames { get; }
    public string LastNames { get; }

    private PersonName(string firstNames, string lastNames)
    {
        FirstNames = firstNames;
        LastNames = lastNames;
    }

    public static PersonName Create(string firstNames, string lastNames)
        => new(Require(firstNames, "Los nombres son obligatorios."), Require(lastNames, "Los apellidos son obligatorios."));

    internal static PersonName FromStorage(string firstNames, string lastNames) => new(firstNames, lastNames);

    public override string ToString() => $"{FirstNames} {LastNames}";

    private static string Require(string value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(message);

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException($"Los nombres y apellidos no pueden superar {MaxLength} caracteres.");

        return normalized;
    }
}
