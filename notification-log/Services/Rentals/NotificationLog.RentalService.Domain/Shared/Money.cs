using System.Globalization;
using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Shared;

public sealed record Money
{
    private static readonly CultureInfo Colombia = CultureInfo.GetCultureInfo("es-CO");

    public long Amount { get; }

    private Money(long amount) => Amount = amount;

    public static Money Create(long amount)
    {
        if (amount < 0)
            throw new DomainException("Un valor en pesos no puede ser negativo.");

        return new Money(amount);
    }

    internal static Money FromStorage(long amount) => new(amount);

    public override string ToString() => "$" + Amount.ToString("N0", Colombia);
}
