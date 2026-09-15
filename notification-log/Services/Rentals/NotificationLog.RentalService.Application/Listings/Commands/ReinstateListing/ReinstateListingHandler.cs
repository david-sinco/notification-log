using Application.Shared.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;

public sealed class ReinstateListingHandler
{
    private readonly IListingRepository _listings;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public ReinstateListingHandler(IListingRepository listings, IUnitOfWork uow, TimeProvider time)
        => (_listings, _uow, _time) = (listings, uow, time);

    public async Task HandleAsync(ReinstateListingCommand cmd, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        listing.Reinstate(cmd.ModeratorId, _time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
