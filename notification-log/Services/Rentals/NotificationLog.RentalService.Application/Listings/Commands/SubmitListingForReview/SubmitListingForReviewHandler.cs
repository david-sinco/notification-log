using System.Security.Claims;
using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;

public sealed class SubmitListingForReviewHandler
{
    private readonly IListingRepository _listings;
    private readonly IUnitOfWork _uow;

    public SubmitListingForReviewHandler(IListingRepository listings, IUnitOfWork uow)
        => (_listings, _uow) = (listings, uow);

    public async Task HandleAsync(SubmitListingForReviewCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        ListingAccess.EnsureCanManage(listing, user);

        listing.SubmitForReview();

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
