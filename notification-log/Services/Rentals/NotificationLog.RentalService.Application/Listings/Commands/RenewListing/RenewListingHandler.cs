using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.RenewListing;

public sealed class RenewListingHandler(IListingRepository listings, IUnitOfWork uow, TimeProvider time)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(RenewListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        Listing.EnsureCanManage(listing, user);

        listing.Renew(_time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
