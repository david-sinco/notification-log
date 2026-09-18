using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;

public sealed class ChangeListingPriceHandler
{
    private readonly IListingRepository _listings;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ChangeListingPriceCommand> _validator;
    private readonly TimeProvider _time;

    public ChangeListingPriceHandler(
        IListingRepository listings, IUnitOfWork uow, IValidator<ChangeListingPriceCommand> validator, TimeProvider time)
        => (_listings, _uow, _validator, _time) = (listings, uow, validator, time);

    public async Task HandleAsync(ChangeListingPriceCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        ListingAccess.EnsureCanManage(listing, user);

        listing.ChangePrice(Money.Create(cmd.Price), _time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
