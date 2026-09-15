using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Offers.ValueObjects;

public sealed record RentTerms
{
    public DateOnly StartDate { get; }
    public int Months { get; }

    private RentTerms(DateOnly startDate, int months)
    {
        StartDate = startDate;
        Months = months;
    }

    public static RentTerms Create(DateOnly startDate, int months, DateOnly today)
    {
        if (startDate < today.AddDays(OfferPolicy.MinDaysUntilRentStart))
            throw new DomainException(
                $"El arriendo debe empezar al menos {OfferPolicy.MinDaysUntilRentStart} días después de la oferta.");

        if (startDate > today.AddDays(OfferPolicy.MaxDaysUntilRentStart))
            throw new DomainException(
                $"El arriendo no puede empezar más de {OfferPolicy.MaxDaysUntilRentStart} días después de la oferta.");

        if (months is < OfferPolicy.MinRentMonths or > OfferPolicy.MaxRentMonths)
            throw new DomainException(
                $"La duración del arriendo debe estar entre {OfferPolicy.MinRentMonths} y {OfferPolicy.MaxRentMonths} meses.");

        return new RentTerms(startDate, months);
    }

    internal static RentTerms FromStorage(DateOnly startDate, int months) => new(startDate, months);
}
