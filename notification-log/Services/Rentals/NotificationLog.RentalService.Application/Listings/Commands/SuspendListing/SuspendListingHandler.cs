using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;

public sealed class SuspendListingHandler(
    IListingRepository listings,
    IOwnerRepository owners,
    IUnitOfWork uow,
    INotificationProducer notifications,
    IValidator<SuspendListingCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly IValidator<SuspendListingCommand> _validator = validator;

    public async Task HandleAsync(SuspendListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        listing.Suspend(cmd.Reason);

        if (listing.DomainEvents.Count == 0)
            return;

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        var owner = await _owners.GetAsync(listing.OwnerId, ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.Suspended,
            owner.HostUserId,
            new Dictionary<string, string>(ListingNotificationData.For(listing))
            {
                ["reason"] = cmd.Reason.Trim()
            },
            ct);
    }
}
