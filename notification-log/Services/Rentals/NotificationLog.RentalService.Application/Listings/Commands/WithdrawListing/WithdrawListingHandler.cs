using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;

public sealed class WithdrawListingHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<WithdrawListingCommand> _validator;

    public WithdrawListingHandler(
        IListingRepository listings, ListingAccess access, IUnitOfWork uow, IValidator<WithdrawListingCommand> validator)
        => (_listings, _access, _uow, _validator) = (listings, access, uow, validator);

    public async Task HandleAsync(WithdrawListingCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        if (!cmd.ByModerator)
            await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        listing.Withdraw(cmd.Reason);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
