using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Events;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ListingNotificationsProcess
{
    private readonly IListingRepository _listings;
    private readonly IVisitRepository _visits;
    private readonly INotificationDispatcher _notifications;

    public ListingNotificationsProcess(
        IListingRepository listings,
        IVisitRepository visits,
        INotificationDispatcher notifications)
        => (_listings, _visits, _notifications) = (listings, visits, notifications);

    public Task NotifyAsync(Guid streamId, IDomainEvent domainEvent, CancellationToken ct) => domainEvent switch
    {
        ListingApproved => ToHostAsync(streamId, NotificationKeys.ListingApproved, ct),
        ListingRejected e => ToHostAsync(streamId, NotificationKeys.ListingRejected, ct, ("reasons", string.Join(",", e.Reasons))),
        ListingExpired => ToHostAsync(streamId, NotificationKeys.ListingExpired, ct),
        ListingSuspended e => ToHostAsync(streamId, NotificationKeys.ListingSuspended, ct, ("reason", e.Reason)),
        ListingClosed => ToHostAsync(streamId, NotificationKeys.ListingClosed, ct),
        VisitRequested e => ToHostAsync(e.ListingId, NotificationKeys.VisitRequested, ct, ("visit_id", e.VisitId.ToString())),
        VisitConfirmed or VisitDeclined or VisitCancelled => OnVisitChangedAsync(streamId, domainEvent, ct),
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

    private async Task OnVisitChangedAsync(Guid visitId, IDomainEvent domainEvent, CancellationToken ct)
    {
        if (await _visits.LoadAsync(visitId, ct) is not { } visit)
            return;

        var (key, recipient) = domainEvent switch
        {
            VisitConfirmed => (NotificationKeys.VisitConfirmed, visit.VisitorId),
            VisitDeclined => (NotificationKeys.VisitDeclined, visit.VisitorId),
            VisitCancelled c => (NotificationKeys.VisitCancelled, c.By == VisitParty.Visitor ? visit.HostId : visit.VisitorId),
            _ => throw new ArgumentOutOfRangeException(nameof(domainEvent))
        };

        await _notifications.DispatchAsync(
            key,
            recipient,
            new Dictionary<string, string>
            {
                ["visit_id"] = visitId.ToString(),
                ["listing_id"] = visit.ListingId.ToString()
            },
            ct);
    }
}
