using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Shared;

public sealed record Stratum
{
    public const int Min = 1;
    public const int Max = 6;

    public int Value { get; }

    private Stratum(int value) => Value = value;

    public static Stratum Create(int value)
    {
        if (value is < Min or > Max)
            throw new DomainException($"El estrato debe estar entre {Min} y {Max}.");

        return new Stratum(value);
    }

    internal static Stratum FromStorage(int value) => new(value);

    public override string ToString() => Value.ToString();
}
