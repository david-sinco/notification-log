using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Application.Common.Producers;

namespace NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;

public sealed class WithdrawListingHandler(
    IListingRepository listings,
    IOwnerRepository owners,
    IUnitOfWork uow,
    INotificationProducer notifications,
    IValidator<WithdrawListingCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly IValidator<WithdrawListingCommand> _validator = validator;

    public async Task HandleAsync(WithdrawListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        var owner = await _owners.GetAsync(listing.OwnerId, ct);
        listing.EnsureCanManage(user, owner.RelatedUserId);

        listing.Withdraw(cmd.Reason);

        if (listing.DomainEvents.Count == 0)
            return;

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.Withdrawn,
            owner.HostUserId,
            new Dictionary<string, string>(ListingNotificationData.For(listing))
            {
                ["reason"] = cmd.Reason.Trim()
            },
            ct);
    }
}
