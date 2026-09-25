using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.Application.Listings.Commands.ApproveListing;

public sealed class ApproveListingHandler(IListingRepository listings, IUnitOfWork uow, TimeProvider time)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(ApproveListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        ListingAccess.EnsureStaff(user);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        listing.Approve(user.GetUserId(), _time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
