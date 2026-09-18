using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.SetListingAvailability;

public sealed class SetListingAvailabilityHandler
{
    private readonly IListingRepository _listings;
    private readonly IUnitOfWork _uow;

    public SetListingAvailabilityHandler(
        IListingRepository listings, IUnitOfWork uow)
        => (_listings, _uow) = (listings, uow);

    public async Task HandleAsync(SetListingAvailabilityCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        ListingAccess.EnsureCanManage(listing, user);

        if (cmd.IsAvailable) listing.Resume();
        else listing.Pause();

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
