using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;

public sealed class ExpireListingHandler(IListingRepository listings, IUnitOfWork uow, TimeProvider time)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(ExpireListingCommand cmd, CancellationToken ct)
    {
        if (await _listings.LoadAsync(cmd.ListingId, ct) is not { } listing)
            return;

        listing.Expire(_time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
