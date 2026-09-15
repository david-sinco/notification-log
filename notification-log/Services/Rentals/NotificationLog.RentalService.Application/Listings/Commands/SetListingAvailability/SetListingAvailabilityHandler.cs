using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.SetListingAvailability;

public sealed class SetListingAvailabilityHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;

    public SetListingAvailabilityHandler(IListingRepository listings, ListingAccess access, IUnitOfWork uow)
        => (_listings, _access, _uow) = (listings, access, uow);

    public async Task HandleAsync(SetListingAvailabilityCommand cmd, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        if (cmd.IsAvailable) listing.Resume();
        else listing.Pause();

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
