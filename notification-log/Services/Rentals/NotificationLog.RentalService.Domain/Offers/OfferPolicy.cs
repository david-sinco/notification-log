namespace NotificationLog.RentalService.Domain.Offers;

public static class OfferPolicy
{
    public const decimal MinOfferRatio = 0.80m;
    public const int MaxCounterOffers = 3;
    public const int MinDaysUntilRentStart = 7;
    public const int MaxDaysUntilRentStart = 90;
    public const int MinRentMonths = 6;
    public const int MaxRentMonths = 36;
    public const int MaxReasonLength = 500;

    public static readonly TimeSpan ResponseWindow = TimeSpan.FromHours(72);
}
