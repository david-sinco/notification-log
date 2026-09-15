using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;

public sealed record PriceRange
{
    public Money? Min { get; }
    public Money? Max { get; }

    private PriceRange(Money? min, Money? max)
    {
        Min = min;
        Max = max;
    }

    public static PriceRange Create(Money? min, Money? max)
    {
        if (min is null && max is null)
            throw new DomainException("El rango de precio necesita un mínimo, un máximo o ambos.");

        if (min is not null && max is not null && min.Amount > max.Amount)
            throw new DomainException("El precio mínimo no puede ser mayor que el máximo.");

        return new PriceRange(min, max);
    }

    internal static PriceRange FromStorage(Money? min, Money? max) => new(min, max);
}
