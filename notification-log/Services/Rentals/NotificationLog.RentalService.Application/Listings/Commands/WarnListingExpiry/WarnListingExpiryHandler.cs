using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Processes;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Listings.Commands.WarnListingExpiry;

public sealed class WarnListingExpiryHandler
{
    private readonly IListingRepository _listings;
    private readonly INotificationDispatcher _notifications;

    public WarnListingExpiryHandler(IListingRepository listings, INotificationDispatcher notifications)
        => (_listings, _notifications) = (listings, notifications);

    public async Task HandleAsync(WarnListingExpiryCommand cmd, CancellationToken ct)
    {
        var listing = await _listings.LoadAsync(cmd.ListingId, ct);

        if (listing is null
            || listing.ExpiresAt != cmd.ExpiresAt
            || listing.Status is not (ListingStatus.Published or ListingStatus.Paused))
            return;

        await _notifications.DispatchAsync(
            NotificationKeys.ListingExpiresSoon,
            ListingAccess.HostOf(listing),
            new Dictionary<string, string>
            {
                ["listing_id"] = listing.Id.ToString(),
                ["expires_at"] = cmd.ExpiresAt.ToString("O")
            },
            ct);
    }
}
