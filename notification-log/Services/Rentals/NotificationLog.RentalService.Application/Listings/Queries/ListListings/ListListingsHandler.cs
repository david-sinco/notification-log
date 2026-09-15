using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;

namespace NotificationLog.RentalService.Application.Listings.Queries.ListListings;

public sealed class ListListingsHandler
{
    private readonly IListingReadModel _listings;

    public ListListingsHandler(IListingReadModel listings) => _listings = listings;

    public async Task<PagedResult<ListingSummaryDto>> HandleAsync(ListListingsQuery query, CancellationToken ct)
    {
        var paged = query with { Page = Paging.Page(query.Page), PageSize = Paging.Size(query.PageSize) };
        var (items, total) = await _listings.ListAsync(paged, ct);

        return new PagedResult<ListingSummaryDto>(items, paged.Page, paged.PageSize, total);
    }
}
