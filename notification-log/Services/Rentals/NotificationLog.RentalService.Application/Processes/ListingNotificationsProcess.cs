using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Events;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ListingNotificationsProcess
{
    private readonly IListingRepository _listings;
    private readonly INotificationDispatcher _notifications;

    public ListingNotificationsProcess(
        IListingRepository listings,
        INotificationDispatcher notifications)
        => (_listings, _notifications) = (listings, notifications);

    public Task NotifyAsync(Guid streamId, IDomainEvent domainEvent, CancellationToken ct) => domainEvent switch
    {
        ListingApproved => ToHostAsync(streamId, NotificationKeys.ListingApproved, ct),
        ListingRejected e => ToHostAsync(streamId, NotificationKeys.ListingRejected, ct, ("reasons", string.Join(",", e.Reasons))),
        ListingExpired => ToHostAsync(streamId, NotificationKeys.ListingExpired, ct),
        ListingSuspended e => ToHostAsync(streamId, NotificationKeys.ListingSuspended, ct, ("reason", e.Reason)),
        ListingClosed => ToHostAsync(streamId, NotificationKeys.ListingClosed, ct),
        _ => Task.CompletedTask
    };

    private async Task ToHostAsync(Guid listingId, string key, CancellationToken ct, params (string Key, string Value)[] extra)
    {
        if (await _listings.LoadAsync(listingId, ct) is not { } listing)
            return;

        var data = extra.ToDictionary(item => item.Key, item => item.Value);
        data["listing_id"] = listingId.ToString();

        await _notifications.DispatchAsync(key, ListingAccess.HostOf(listing), data, ct);
    }
}
