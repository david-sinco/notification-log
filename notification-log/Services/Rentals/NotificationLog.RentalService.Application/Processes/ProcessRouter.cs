using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Listings.Events;
using NotificationLog.RentalService.Domain.Visits.Events;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ProcessRouter
{
    private readonly ListingLifecycleProcess _lifecycle;
    private readonly VisitProcess _visits;
    private readonly ListingNotificationsProcess _notifications;

    public ProcessRouter(
        ListingLifecycleProcess lifecycle,
        VisitProcess visits,
        ListingNotificationsProcess notifications)
        => (_lifecycle, _visits, _notifications) = (lifecycle, visits, notifications);

    public async Task RouteAsync(Guid streamId, IDomainEvent domainEvent, CancellationToken ct)
    {
        switch (domainEvent)
        {
            case ListingApproved e:
                await _lifecycle.OnPublishedAsync(streamId, e.ExpiresAt, ct);
                break;

            case ListingRenewed e:
                await _lifecycle.OnPublishedAsync(streamId, e.ExpiresAt, ct);
                break;

            case ListingClosed or ListingWithdrawn:
                await _lifecycle.OnNoLongerAvailableAsync(streamId, ct);
                break;

            case VisitRequested e:
                await _visits.OnRequestedAsync(streamId, e.RespondBy, ct);
                break;

            case VisitConfirmed e:
                await _visits.OnConfirmedAsync(streamId, e.SlotStart, ct);
                break;
        }

        await _notifications.NotifyAsync(streamId, domainEvent, ct);
    }
}
