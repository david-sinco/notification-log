using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.PauseListing;

public sealed class PauseListingHandler(IListingRepository listings, IUnitOfWork uow)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;

    public async Task HandleAsync(PauseListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        ListingAccess.EnsureCanManage(listing, user);

        listing.Pause();

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
