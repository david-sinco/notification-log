using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Abstractions;

namespace NotificationLog.RentalService.Application.Listings.Queries.ListListings;

public sealed class ListListingsHandler
{
    private readonly IListingReadModel _listings;

    public ListListingsHandler(IListingReadModel listings) => _listings = listings;

    public async Task<PagedResult<ListingSummaryDto>> HandleAsync(ListListingsQuery query, PageRequest paging, CancellationToken ct)
    {
        var (items, total) = await _listings.ListAsync(query, paging, ct);

        return new PagedResult<ListingSummaryDto>(items, paging, total);
    }
}
