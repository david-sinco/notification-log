using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Application.Common.Producers;

namespace NotificationLog.RentalService.Application.Listings.Commands.ResumeListing;

public sealed class ResumeListingHandler(IListingRepository listings, IOwnerRepository owners, IUnitOfWork uow, INotificationProducer notifications)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;

    public async Task HandleAsync(ResumeListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        var owner = await _owners.GetAsync(listing.OwnerId, ct);
        listing.EnsureCanManage(user, owner.RelatedUserId);

        listing.Resume();

        if (listing.DomainEvents.Count == 0)
            return;

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.Resumed,
            owner.HostUserId,
            ListingNotificationData.For(listing),
            ct);
    }
}
