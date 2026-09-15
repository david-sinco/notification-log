using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.RenewListing;

public sealed class RenewListingHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public RenewListingHandler(IListingRepository listings, ListingAccess access, IUnitOfWork uow, TimeProvider time)
        => (_listings, _access, _uow, _time) = (listings, access, uow, time);

    public async Task HandleAsync(RenewListingCommand cmd, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        listing.Renew(_time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
