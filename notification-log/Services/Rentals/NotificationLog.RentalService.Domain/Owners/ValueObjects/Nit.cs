using System.Text.RegularExpressions;
using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Owners.ValueObjects;

public sealed record Nit
{
    private static readonly Regex Pattern = new(@"^(\d{8,9})-(\d)$", RegexOptions.Compiled);
    private static readonly int[] Weights = [3, 7, 13, 17, 19, 23, 29, 37, 41];

    public string Number { get; }
    public int CheckDigit { get; }

    private Nit(string number, int checkDigit)
    {
        Number = number;
        CheckDigit = checkDigit;
    }

    public static Nit Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El NIT es obligatorio.");

        var match = Pattern.Match(value.Replace(".", string.Empty).Replace(" ", string.Empty));

        if (!match.Success)
            throw new DomainException("El NIT debe tener el formato 900123456-7.");

        var number = match.Groups[1].Value;
        var checkDigit = int.Parse(match.Groups[2].Value);

        if (CalculateCheckDigit(number) != checkDigit)
            throw new DomainException("El dígito de verificación del NIT no es correcto.");

        return new Nit(number, checkDigit);
    }

    internal static Nit FromStorage(string number, int checkDigit) => new(number, checkDigit);

    public override string ToString() => $"{Number}-{CheckDigit}";

    private static int CalculateCheckDigit(string number)
    {
        var sum = number.Reverse().Select((digit, i) => (digit - '0') * Weights[i]).Sum();
        var remainder = sum % 11;

        return remainder > 1 ? 11 - remainder : remainder;
    }
}
