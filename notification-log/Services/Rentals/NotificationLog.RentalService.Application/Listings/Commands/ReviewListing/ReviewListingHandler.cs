using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReviewListing;

public sealed class ReviewListingHandler
{
    private readonly IListingRepository _listings;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ReviewListingCommand> _validator;
    private readonly TimeProvider _time;

    public ReviewListingHandler(
        IListingRepository listings, IUnitOfWork uow, IValidator<ReviewListingCommand> validator, TimeProvider time)
        => (_listings, _uow, _validator, _time) = (listings, uow, validator, time);

    public async Task HandleAsync(ReviewListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        ListingAccess.EnsureStaff(user);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        if (cmd.Approve)
            listing.Approve(user.GetUserId(), _time.GetUtcNow());
        else
            listing.Reject(user.GetUserId(), cmd.Reasons);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
