using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Application.Common.Producers;

namespace NotificationLog.RentalService.Application.Listings.Commands.CloseListing;

public sealed class CloseListingHandler(
    IListingRepository listings,
    IOwnerRepository owners,
    IUnitOfWork uow,
    INotificationProducer notifications,
    IValidator<CloseListingCommand> validator,
    TimeProvider time)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly IValidator<CloseListingCommand> _validator = validator;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(CloseListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        var owner = await _owners.GetAsync(listing.OwnerId, ct);
        listing.EnsureCanManage(user, owner.RelatedUserId);

        listing.Close(Money.Create(cmd.FinalPrice), cmd.SignedOn, _time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.Closed,
            owner.HostUserId,
            new Dictionary<string, string>(ListingNotificationData.For(listing))
            {
                ["final_price"] = Money.Create(cmd.FinalPrice).ToString(),
                ["signed_on"] = cmd.SignedOn.ToString("yyyy-MM-dd")
            },
            ct);
    }
}
