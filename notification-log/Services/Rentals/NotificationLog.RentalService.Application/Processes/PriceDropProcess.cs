using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class PriceDropProcess
{
    private readonly IListingRepository _listings;
    private readonly IProcessLookups _lookups;
    private readonly INotificationDispatcher _notifications;
    private readonly SavedSearchMatchingProcess _matching;

    public PriceDropProcess(
        IListingRepository listings,
        IProcessLookups lookups,
        INotificationDispatcher notifications,
        SavedSearchMatchingProcess matching)
        => (_listings, _lookups, _notifications, _matching) = (listings, lookups, notifications, matching);

    public async Task OnPriceChangedAsync(Guid listingId, long oldPrice, long newPrice, CancellationToken ct)
    {
        if (oldPrice <= 0 || newPrice >= oldPrice)
            return;

        if ((decimal)(oldPrice - newPrice) / oldPrice < ListingPolicy.PriceDropNoticeThreshold)
            return;

        if (await _listings.LoadAsync(listingId, ct) is not { Status: ListingStatus.Published })
            return;

        var data = new Dictionary<string, string>
        {
            ["listing_id"] = listingId.ToString(),
            ["old_price"] = oldPrice.ToString(),
            ["new_price"] = newPrice.ToString()
        };

        foreach (var userId in await _lookups.FindFavoritedByAsync(listingId, ct))
            await _notifications.DispatchAsync(NotificationKeys.FavoritePriceDrop, userId, data, ct);

        await _matching.NotifyImmediateMatchesAsync(listingId, ct);
    }
}
