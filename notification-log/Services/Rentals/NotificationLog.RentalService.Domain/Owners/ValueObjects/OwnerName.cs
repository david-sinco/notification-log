using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Owners.ValueObjects;

public sealed record OwnerName
{
    public const int MaxLength = 200;

    public string Value { get; }

    private OwnerName(string value) => Value = value;

    public static OwnerName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("El nombre del propietario es obligatorio.");

        var normalized = value.Trim();

        if (normalized.Length > MaxLength)
            throw new DomainException($"El nombre del propietario no puede superar {MaxLength} caracteres.");

        return new OwnerName(normalized);
    }

    internal static OwnerName FromStorage(string value) => new(value);

    public override string ToString() => Value;
}
