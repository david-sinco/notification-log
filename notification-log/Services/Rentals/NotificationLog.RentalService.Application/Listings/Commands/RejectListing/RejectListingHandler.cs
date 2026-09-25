using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.Application.Listings.Commands.RejectListing;

public sealed class RejectListingHandler(
    IListingRepository listings,
    IUnitOfWork uow,
    IValidator<RejectListingCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<RejectListingCommand> _validator = validator;

    public async Task HandleAsync(RejectListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        ListingAccess.EnsureStaff(user);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        listing.Reject(user.GetUserId(), cmd.Reasons);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
