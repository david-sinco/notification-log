using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Application.Common;

namespace NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;

public sealed class ExpireListingHandler(IListingRepository listings, IOwnerRepository owners, IUnitOfWork uow, INotificationProducer notifications, TimeProvider time)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(ExpireListingCommand cmd, CancellationToken ct)
    {
        if (await _listings.LoadAsync(cmd.ListingId, ct) is not { } listing)
            return;

        listing.Expire(_time.GetUtcNow());

        if (listing.DomainEvents.Count == 0)
            return;

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        var owner = await _owners.GetAsync(listing.OwnerId, ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.Expired,
            owner.HostUserId,
            ListingNotificationData.For(listing),
            ct);
    }
}
