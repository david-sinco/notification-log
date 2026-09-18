using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Owners.ValueObjects;

public sealed record LegalName
{
    public const int MaxLength = 200;

    public string Value { get; }

    private LegalName(string value) => Value = value;

    public static LegalName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("La razón social es obligatoria.");

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException($"La razón social no puede superar {MaxLength} caracteres.");

        return new LegalName(normalized);
    }

    internal static LegalName FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
