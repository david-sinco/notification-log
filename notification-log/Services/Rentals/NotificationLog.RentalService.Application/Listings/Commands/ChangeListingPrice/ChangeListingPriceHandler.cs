using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Application.Common.Producers;

namespace NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;

public sealed class ChangeListingPriceHandler(
    IListingRepository listings,
    IOwnerRepository owners,
    IUnitOfWork uow,
    INotificationProducer notifications,
    IValidator<ChangeListingPriceCommand> validator,
    TimeProvider time)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly IValidator<ChangeListingPriceCommand> _validator = validator;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(ChangeListingPriceCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        var owner = await _owners.GetAsync(listing.OwnerId, ct);
        listing.EnsureCanManage(user, owner.RelatedUserId);

        var oldPrice = listing.Price;

        listing.ChangePrice(Money.Create(cmd.Price), _time.GetUtcNow());

        if (listing.DomainEvents.Count == 0)
            return;

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            ListingNotificationKeys.PriceChanged,
            owner.HostUserId,
            new Dictionary<string, string>(ListingNotificationData.For(listing))
            {
                ["old_price"] = ListingNotificationData.PriceOf(oldPrice),
                ["new_price"] = ListingNotificationData.PriceOf(listing.Price)
            },
            ct);
    }
}
