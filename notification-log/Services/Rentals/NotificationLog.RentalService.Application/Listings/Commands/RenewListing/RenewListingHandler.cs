using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Application.Common.Producers;

namespace NotificationLog.RentalService.Application.Listings.Commands.RenewListing;

public sealed class RenewListingHandler(IListingRepository listings, IOwnerRepository owners, IUnitOfWork uow, INotificationProducer notifications, TimeProvider time)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(RenewListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        var owner = await _owners.GetAsync(listing.OwnerId, ct);
        listing.EnsureCanManage(user, owner.RelatedUserId);

        listing.Renew(_time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.Renewed,
            owner.HostUserId,
            new Dictionary<string, string>(ListingNotificationData.For(listing))
            {
                ["expires_at"] = ListingNotificationData.DateOf(listing.ExpiresAt!.Value)
            },
            ct);
    }
}
