using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;

public sealed class ReinstateListingHandler(IListingRepository listings, IOwnerRepository owners, IUnitOfWork uow, INotificationProducer notifications, TimeProvider time)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(ReinstateListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        listing.Reinstate(user.GetUserId(), _time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        var owner = await _owners.GetAsync(listing.OwnerId, ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.Reinstated,
            owner.HostUserId,
            ListingNotificationData.For(listing),
            ct);
    }
}
