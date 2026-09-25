using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Listings.Events;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class ProcessRouter
{
    private readonly ListingLifecycleProcess _lifecycle;
    private readonly ListingNotificationsProcess _notifications;

    public ProcessRouter(
        ListingLifecycleProcess lifecycle,
        ListingNotificationsProcess notifications)
        => (_lifecycle, _notifications) = (lifecycle, notifications);

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
        }

        await _notifications.NotifyAsync(streamId, domainEvent, ct);
    }
}
