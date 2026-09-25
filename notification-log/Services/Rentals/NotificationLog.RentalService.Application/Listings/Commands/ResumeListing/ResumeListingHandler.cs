using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.ResumeListing;

public sealed class ResumeListingHandler(IListingRepository listings, IUnitOfWork uow)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;

    public async Task HandleAsync(ResumeListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        Listing.EnsureCanManage(listing, user);

        listing.Resume();

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
