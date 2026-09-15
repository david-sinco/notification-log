using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;

public sealed class ExpireListingHandler
{
    private readonly IListingRepository _listings;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public ExpireListingHandler(IListingRepository listings, IUnitOfWork uow, TimeProvider time)
        => (_listings, _uow, _time) = (listings, uow, time);

    public async Task HandleAsync(ExpireListingCommand cmd, CancellationToken ct)
    {
        if (await _listings.LoadAsync(cmd.ListingId, ct) is not { } listing)
            return;

        listing.Expire(_time.GetUtcNow());

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
