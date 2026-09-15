using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Listings;

public static class ListingPolicy
{
    public const int MinPhotos = 5;
    public const int MaxPhotos = 30;
    public const int MinDescriptionLength = 100;
    public const int MaxDescriptionLength = 2000;
    public const int MaxReasonLength = 500;
    public const decimal MinArea = 20m;
    public const long MinRentPrice = 300_000;
    public const long MinSalePrice = 30_000_000;
    public const int ReportsToSuspend = 3;
    public const int MaxActiveListingsPerOwner = 3;
    public const decimal PriceDropNoticeThreshold = 0.03m;

    public static readonly TimeSpan Validity = TimeSpan.FromDays(60);
    public static readonly TimeSpan ExpiryNotice = TimeSpan.FromDays(7);
    public static readonly TimeSpan RenewalWindow = TimeSpan.FromDays(7);
    public static readonly TimeSpan PriceChangeCooldown = TimeSpan.FromHours(24);
    public static readonly TimeSpan ReservationDuration = TimeSpan.FromDays(10);
    public static readonly TimeSpan ReservationExtension = TimeSpan.FromDays(5);

    public static long MinimumPriceFor(Operation operation)
        => operation == Operation.Rent ? MinRentPrice : MinSalePrice;
}
